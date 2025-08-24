/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise repositories on Azure DevOps
 * with a two-way synchronization.
 * Copyright (C) 2025  Andrea Lucchese
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Services.Common;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using User = ADAtickets.Shared.Models.User;

namespace ADAtickets.ApiService.Controllers;

/// <summary>
///     Handles the incoming webhooks from Azure DevOps to create, update or delete tickets.
/// </summary>
/// <param name="ticketRepository">Object defining the operations allowed on the <see cref="Ticket"/> entity type. </param>
/// <param name="platformRepository">Object defining the operations allowed on the <see cref="Platform"/> entity type.</param>
/// <param name="userRepository">Object defining the operations allowed on the <see cref="User"/> entity type.</param>
/// <param name="configuration">Configuration object containing the application settings.</param>
/// <param name="ticketsController">Controller managing the tickets.</param>
/// <param name="notificationsController">Controller managing the notifications.</param>
[Route($"v{Service.APIVersion}/webhook")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Authorize(Policy = Policy.WebHook, AuthenticationSchemes = Scheme.AzureDevOpsDefault)]
[FormatFilter]
public sealed class WebhookController(
    ITicketRepository ticketRepository,
    IPlatformRepository platformRepository,
    IUserRepository userRepository,
    IConfiguration configuration,
    TicketsController ticketsController,
    NotificationsController notificationsController
) : ControllerBase
{
    /// <summary>
    ///     Handles the incoming webhook from Azure DevOps when a work item is created to create a new ticket.
    /// </summary>
    /// <param name="payload">The content of the Azure DevOps request body.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="ActionResult" /> that incapsulates the call result.</returns>
    [HttpPost("ticket/created")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostTicket([FromBody] AzureDevOpsWebHookResponseDto payload)
    {
        // Check if the item has all the required fields
        if (!IsCreatePayloadValid(payload))
        {
            return BadRequest();
        }

        // Check if this work item was created by ADAtickets API to avoid infinite loops
        if (IsCreatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket already exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), payload.Resource.Id!.Value.ToString() } }
        )).FirstOrDefault();

        if (existingTicket is not null)
        {
            return Conflict();
        }

        // Create new ticket from work item
        var createdTicket = await CreateTicketFromWorkItemAsync(payload);

        // Create notifications, assign the ticket and create the first edits.
        await ticketsController.ProcessTicketCreationAsync(createdTicket, includeAzureDevOpsOperations: false);

        return Created();
    }

    /// <summary>
    ///     Handles the incoming webhook from Azure DevOps when a work item is updated to update a existing ticket.
    /// </summary>
    /// <param name="payload">The content of the Azure DevOps request body.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="ActionResult" /> that incapsulates the call result.</returns>
    [HttpPost("ticket/updated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutTicket([FromBody] AzureDevOpsWebHookResponseDto payload)
    {
        // Check if the item has all the required fields
        if (!IsUpdatePayloadValid(payload))
        {
            return BadRequest();
        }

        // Check if this work item was edited by ADAtickets API to avoid infinite loops
        if (IsUpdatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), payload.Resource.Id!.Value.ToString() } }
        )).FirstOrDefault();

        if (existingTicket is null)
        {
            return NotFound();
        }

        // Keep the old operator for notification purposes.
        var oldAssignedOperator = existingTicket.OperatorUserId;
        // Keep the old status for edits purposes.
        var oldStatus = existingTicket.Status;

        // Update ticket from work item
        var updatedTicket = await UpdateTicketFromWorkItemAsync(payload, existingTicket);

        // Fetch the requester id.
        var requesterId = await GetUserIdFromWorkItemAsync(payload);

        // Create notification and create the edit.
        await ticketsController.ProcessTicketUpdateAsync(updatedTicket, oldStatus, requesterId, includeAzureDevOpsOperations: false);

        // If the operator was changed, create a notification and an edit.
        if (oldAssignedOperator != updatedTicket.OperatorUserId)
            await ticketsController.ProcessTicketOperatorUpdateAsync(updatedTicket, oldAssignedOperator, requesterId, includeAzureDevOpsOperations: false);

        return NoContent();
    }

    /// <summary>
    ///     Handles the incoming webhook from Azure DevOps when a work item is deleted to delete an existing ticket.
    /// </summary>
    /// <param name="payload">The content of the Azure DevOps request body.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="ActionResult" /> that incapsulates the call result.</returns>
    [HttpPost("ticket/deleted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket([FromBody] AzureDevOpsWebHookResponseDto payload)
    {
        // Check if the item has all the required fields
        if (!IsDeletePayloadValid(payload))
        {
            return BadRequest();
        }

        // Check if this work item was edited by ADAtickets API to avoid infinite loops
        if (IsUpdatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), payload.Resource.Id!.Value.ToString() } }
        )).FirstOrDefault();

        if (existingTicket is null)
        {
            return NotFound();
        }

        // Save the UserNotifications before deletion in order to send the signals
        var userNotificationsToSignal = await notificationsController.RetrieveAllTicketUserNotificationsAsync(existingTicket.Notifications);

        await ticketRepository.DeleteTicketAsync(existingTicket);

        await ticketsController.ProcessTicketDeletionAsync(existingTicket, userNotificationsToSignal, includeAzureDevOpsOperations: false);

        return NoContent();
    }

    private static bool IsCreatePayloadValid(AzureDevOpsWebHookResponseDto payload)
    {
        return payload.EventType == "workitem.created"
               && payload.Resource.Id.HasValue
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.CreatedDate)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.CreatedBy)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.TeamProject)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.WorkItemType)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Title)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Description)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Priority)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.State);
    }

    private static bool IsUpdatePayloadValid(AzureDevOpsWebHookResponseDto payload)
    {
        return payload.EventType == "workitem.updated"
               && payload.Resource.Id.HasValue
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.ChangedBy)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.TeamProject)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.WorkItemType)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Title)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Description)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.Priority)
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.State);
    }

    private static bool IsDeletePayloadValid(AzureDevOpsWebHookResponseDto payload)
    {
        return payload.EventType == "workitem.deleted"
               && payload.Resource.Id.HasValue
               && payload.Resource.Fields.ContainsKey(AzureDevOpsWebHookResponseDto.Fields.ChangedBy);
    }

    private bool IsCreatedByADAtickets(AzureDevOpsWebHookResponseDto payload)
    {
        var createdBy = payload.Resource.Fields[AzureDevOpsWebHookResponseDto.Fields.CreatedBy].ToString()!;

        return IsADATicketsServicePrincipal(createdBy);
    }

    private bool IsUpdatedByADAtickets(AzureDevOpsWebHookResponseDto payload)
    {
        var changedBy = payload.Resource.Fields[AzureDevOpsWebHookResponseDto.Fields.ChangedBy].ToString()!;

        return IsADATicketsServicePrincipal(changedBy);
    }

    private bool IsADATicketsServicePrincipal(string userIdentifier)
    {
        var servicePrincipalIdentifier = configuration.GetSection("Webhook:ServicePrincipalIdentifier").Value!;

        return userIdentifier.Contains(servicePrincipalIdentifier, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task<Ticket> CreateTicketFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload)
    {
        var workItem = payload.Resource;

        // Find the platform by project name
        var platforms = await platformRepository.GetPlatformsAsync();
        var platform = platforms.First(p => p.Name == workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.TeamProject].ToString());

        // Find the users by user email
        var users = await userRepository.GetUsersAsync();
        var creatorUser = users.First(u => workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.CreatedBy].ToString()!.Contains(u.Email));
        var operatorUser = users.FirstOrDefault(u => (workItem.Fields.GetValueOrDefault(AzureDevOpsWebHookResponseDto.Fields.AssignedTo)?.ToString() ?? "").Contains(u.Email));

        // Clean the description from HTML and Markdown
        var rawDescription = workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Description].ToString()!;
        var cleanDescription = CleanDescription(rawDescription);

        // Map work item to ticket
        var newTicket = new Ticket
        {
            Type = MapTypeFromDevOps(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.WorkItemType].ToString()!),
            CreationDateTime = DateTimeOffset.Parse(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.CreatedDate].ToString()!, CultureInfo.InvariantCulture),
            Title = workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Title].ToString()!,
            Description = cleanDescription,
            Priority = MapPriorityFromDevOps(int.Parse(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Priority].ToString()!)),
            Status = operatorUser is null ? Status.Unassigned : Status.WaitingOperator,
            WorkItemId = workItem.Id!.Value,
            PlatformId = platform.Id,
            CreatorUserId = creatorUser.Id,
            OperatorUserId = operatorUser?.Id
        };

        await ticketRepository.AddTicketAsync(newTicket);

        return newTicket;
    }

    private async Task<Ticket> UpdateTicketFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload, Ticket existingTicket)
    {
        var workItem = payload.Resource;

        // Find the platform by project name
        var platforms = await platformRepository.GetPlatformsAsync();
        var platform = platforms.First(p => p.Name == workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.TeamProject].ToString());

        // Find the users by user email
        var users = await userRepository.GetUsersAsync();
        var operatorUser = users.FirstOrDefault(u => (workItem.Fields.GetValueOrDefault(AzureDevOpsWebHookResponseDto.Fields.AssignedTo)?.ToString() ?? "").Contains(u.Email));

        // Clean the description from HTML and Markdown
        var rawDescription = workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Description].ToString()!;
        var cleanDescription = CleanDescription(rawDescription);

        // Apply only the fields that should be updated from DevOps
        existingTicket.Type = MapTypeFromDevOps(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.WorkItemType].ToString()!);
        existingTicket.Title = workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Title].ToString()!;
        existingTicket.Description = cleanDescription;
        existingTicket.Priority = MapPriorityFromDevOps(int.Parse(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.Priority].ToString()!));
        existingTicket.Status = MapStatusFromDevOps(workItem.Fields[AzureDevOpsWebHookResponseDto.Fields.State].ToString()!);
        existingTicket.PlatformId = platform.Id;
        existingTicket.OperatorUserId = existingTicket.Status == Status.Unassigned ? null : operatorUser?.Id;

        await ticketRepository.UpdateTicketAsync(existingTicket);

        return existingTicket;
    }

    private async Task<Guid> GetUserIdFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload)
    {
        var userIdentity = payload.Resource.Fields[AzureDevOpsWebHookResponseDto.Fields.ChangedBy].ToString()!;
        var userEmail = Regex.Match(userIdentity, "<(?<email>.+?)>", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))
            .Groups["email"]
            .Value;

        var users = await userRepository.GetUsersAsync();
        var user = users.First(u => u.Email == userEmail);

        return user.Id;
    }

    private static string CleanDescription(string rawDescription)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            return string.Empty;
        }

        var cleaned = rawDescription;

        // Decode HTML entities first
        cleaned = WebUtility.HtmlDecode(cleaned);

        // Remove HTML tags
        cleaned = Regex.Replace(cleaned, @"<[^>]+>", string.Empty, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        // Remove Markdown headers
        cleaned = Regex.Replace(cleaned, @"^#{1,6}\s*(.*)$", "$1", RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Remove Markdown bold/italic formatting
        cleaned = Regex.Replace(cleaned, @"\*{1,2}([^*]+)\*{1,2}|_{1,2}([^_]+)_{1,2}", "$1", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        // Remove Markdown links but keep the text
        cleaned = Regex.Replace(cleaned, @"\[([^\]]*)\]\([^)]*\)", "$1", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        // Remove Markdown code blocks
        cleaned = Regex.Replace(cleaned, @"```[\s\S]*?```", string.Empty, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        // Remove Markdown inline code
        cleaned = Regex.Replace(cleaned, @"`([^`]+)`", "$1", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        // Remove Markdown lists markers
        cleaned = Regex.Replace(cleaned, @"^\s*[-*+]\s*(.*)$", "$1", RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(100));

        // Remove excessive whitespace and normalize line breaks
        cleaned = Regex.Replace(cleaned, @"\s+", " ", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        return cleaned.Trim();
    }

    private static Priority MapPriorityFromDevOps(int devOpsPriority)
    {
        return devOpsPriority switch
        {
            < 1 => Priority.High,
            1 => Priority.High,
            2 => Priority.Medium,
            3 => Priority.Low,
            > 3 => Priority.Low,
        };
    }

    private static Status MapStatusFromDevOps(string devOpsState)
    {
        return devOpsState switch
        {
            "To Do" => Status.Unassigned,
            "Doing" => Status.WaitingOperator,
            "Done" => Status.Closed,
            _ => throw new ArgumentOutOfRangeException(nameof(devOpsState), "Invalid work item state.")
        };
    }

    private static TicketType MapTypeFromDevOps(string workItemType)
    {
        return workItemType switch
        {
            "Issue" => TicketType.Bug,
            "Task" or "Epic" => TicketType.Feature,
            _ => throw new ArgumentOutOfRangeException(nameof(workItemType), "Invalid work item type.")
        };
    }
}