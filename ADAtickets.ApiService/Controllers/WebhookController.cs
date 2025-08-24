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
[Authorize(AuthenticationSchemes = Scheme.AzureDevOpsDefault)]
[FormatFilter]
public partial class WebhookController(
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
        // Check if this work item was created by ADAtickets API to avoid infinite loops
        if (IsCreatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket already exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), (payload.Resource.Id ?? 0).ToString() } }
        )).FirstOrDefault();

        if (existingTicket is not null)
        {
            return Conflict();
        }

        // Create new ticket from work item
        if ((await CreateTicketFromWorkItemAsync(payload)) is Ticket createdTicket)
        {
            // Create notifications, assign the ticket and create the first edits.
            await ticketsController.ProcessTicketCreationAsync(createdTicket, includeAzureDevOpsOperations: false);

            return Created();
        }

        return BadRequest();
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
        // Check if this work item was edited by ADAtickets API to avoid infinite loops
        if (IsUpdatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), (payload.Resource.Id ?? 0).ToString() } }
        )).FirstOrDefault();

        if (existingTicket is not null)
        {
            // Keep the old operator for notification purposes.
            var oldAssignedOperator = existingTicket.OperatorUserId;
            // Keep the old status for edits purposes.
            var oldStatus = existingTicket.Status;

            // Update ticket from work item
            if ((await UpdateTicketFromWorkItemAsync(payload, existingTicket)) is Ticket updatedTicket)
            {
                // Fetch the requester id.
                var requesterId = await GetUserIdFromWorkItemAsync(payload);

                // Create notification and create the edit.
                await ticketsController.ProcessTicketUpdateAsync(updatedTicket, oldStatus, requesterId, includeAzureDevOpsOperations: false);

                // If the operator was changed, create a notification and an edit.
                if (oldAssignedOperator != updatedTicket.OperatorUserId)
                    await ticketsController.ProcessTicketOperatorUpdateAsync(updatedTicket, oldAssignedOperator, requesterId, includeAzureDevOpsOperations: false);

                return NoContent();
            }

            return BadRequest();
        }

        return NotFound();
    }

    /// <summary>
    ///     Handles the incoming webhook from Azure DevOps when a work item is delete to delete an existing ticket.
    /// </summary>
    /// <param name="payload">The content of the Azure DevOps request body.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="ActionResult" /> that incapsulates the call result.</returns>
    [HttpPost("ticket/deleted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket([FromBody] AzureDevOpsWebHookResponseDto payload)
    {
        // Check if this work item was edited by ADAtickets API to avoid infinite loops
        if (IsUpdatedByADAtickets(payload))
        {
            return Ok();
        }

        // Check if ticket exists for this work item
        var existingTicket = (await ticketRepository.GetTicketsByAsync(
            new Dictionary<string, string> { { nameof(Ticket.WorkItemId), (payload.Resource.Id ?? 0).ToString() } }
        )).FirstOrDefault();

        if (existingTicket is not null)
        {
            // Save the UserNotifications before deletion in order to send the signals
            var userNotificationsToSignal = await notificationsController.RetrieveAllTicketUserNotificationsAsync([.. existingTicket.Notifications]);

            await ticketsController.ProcessTicketDeletionAsync(existingTicket, userNotificationsToSignal, includeAzureDevOpsOperations: false);

            return NoContent();
        }

        return NotFound();
    }

    private bool IsCreatedByADAtickets(AzureDevOpsWebHookResponseDto payload)
    {
        var createdBy = payload.Resource.Fields["System.CreatedBy"].ToString()!;

        return IsADATicketsServicePrincipal(createdBy);
    }

    private bool IsUpdatedByADAtickets(AzureDevOpsWebHookResponseDto payload)
    {
        var changedBy = payload.Resource.Fields["System.ChangedBy"].ToString()!;

        return IsADATicketsServicePrincipal(changedBy);
    }

    private bool IsADATicketsServicePrincipal(string userIdentifier)
    {
        var servicePrincipalIdentifier = configuration.GetSection("Webhook:ServicePrincipalIdentifier").Value!;

        return userIdentifier.Contains(servicePrincipalIdentifier, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task<Ticket?> CreateTicketFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload)
    {
        try
        {
            var workItem = payload.Resource;

            // Find the platform by project name
            var platforms = await platformRepository.GetPlatformsAsync();
            var platform = platforms.First(p => p.Name == workItem.Fields["System.TeamProject"].ToString());

            // Find the users by user email
            var users = await userRepository.GetUsersAsync();
            var creatorUser = users.First(u => workItem.Fields["System.CreatedBy"].ToString()!.Contains(u.Email));
            var operatorUser = users.FirstOrDefault(u => (workItem.Fields.GetValueOrDefault("System.AssignedTo")?.ToString() ?? "").Contains(u.Email));

            // Map work item to ticket
            var newTicket = new Ticket
            {
                Type = MapTypeFromDevOps(workItem.Fields["System.WorkItemType"].ToString()!),
                Title = workItem.Fields["System.Title"].ToString()!,
                Description = workItem.Fields["System.Description"].ToString()!,
                Priority = MapPriorityFromDevOps(int.Parse(workItem.Fields["Microsoft.VSTS.Common.Priority"].ToString()!)),
                Status = MapStatusFromDevOps(workItem.Fields["System.State"].ToString()!),
                WorkItemId = workItem.Id!.Value,
                PlatformId = platform.Id,
                CreatorUserId = creatorUser.Id,
                OperatorUserId = operatorUser?.Id
            };

            await ticketRepository.AddTicketAsync(newTicket);

            return newTicket;
        }
        catch
        {
            return null;
        }

    }

    private async Task<Ticket?> UpdateTicketFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload, Ticket existingTicket)
    {
        try
        {
            var workItem = payload.Resource;

            // Find the platform by project name
            var platforms = await platformRepository.GetPlatformsAsync();
            var platform = platforms.First(p => p.Name == workItem.Fields["System.TeamProject"].ToString());

            // Find the users by user email
            var users = await userRepository.GetUsersAsync();
            var operatorUser = users.FirstOrDefault(u => (workItem.Fields.GetValueOrDefault("System.AssignedTo")?.ToString() ?? "").Contains(u.Email));

            // Apply only the fields that should be updated from DevOps
            existingTicket.Type = MapTypeFromDevOps(workItem.Fields["System.WorkItemType"].ToString()!);
            existingTicket.Title = workItem.Fields["System.Title"].ToString()!;
            existingTicket.Description = workItem.Fields["System.Description"].ToString()!;
            existingTicket.Priority = MapPriorityFromDevOps(int.Parse(workItem.Fields["Microsoft.VSTS.Common.Priority"].ToString()!));
            existingTicket.Status = MapStatusFromDevOps(workItem.Fields["System.State"].ToString()!);
            existingTicket.PlatformId = platform.Id;
            existingTicket.OperatorUserId = operatorUser?.Id;

            await ticketRepository.UpdateTicketAsync(existingTicket);

            return existingTicket;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Guid> GetUserIdFromWorkItemAsync(AzureDevOpsWebHookResponseDto payload)
    {
        var userIdentity = payload.Resource.Fields["System.ChangedBy"].ToString()!;
        var userEmail = EmailFromFullIdentityRegex().Match(userIdentity).Groups["email"].Value;

        var users = await userRepository.GetUsersAsync();
        var user = users.FirstOrDefault(u => u.Email == userEmail);

        return user?.Id ?? Guid.Empty;
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

    [GeneratedRegex("<(?<email>.+?)>")]
    private static partial Regex EmailFromFullIdentityRegex();
}