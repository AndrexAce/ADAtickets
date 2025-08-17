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

using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Hubs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using System.Net.Mime;
using Controller = ADAtickets.Shared.Constants.Controller;

namespace ADAtickets.ApiService.Controllers;

/// <summary>
///     Web API controller managing requests involving <see cref="Notification" /> etities.
/// </summary>
/// <param name="notificationRepository">
///     Object defining the operations allowed on the <see cref="Notification" /> entity
///     type.
/// </param>
/// <param name="mapper">
///     Object definining the mappings of fields between the <see cref="Notification" /> entity and its
///     <see cref="NotificationRequestDto" /> or <see cref="NotificationResponseDto" /> correspondant.
/// </param>
/// <param name="userNotificationRepository">
///     Object defining the operations allowed on the <see cref="UserNotification" />
///     entity type.
/// </param>
/// <param name="userPlatformRepository">
///     Object defining the operations allowed on the <see cref="UserPlatform" /> entity
///     type.
/// </param>
/// <param name="userRepository">Object defining the operations allowed on the <see cref="User" /> entity type.</param>
/// <param name="notificationsHub">SignalR hub managing the real-time updates of notifications.</param>
[Route($"v{Service.APIVersion}/{Controller.Notifications}")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[FormatFilter]
[ApiConventionType(typeof(ApiConventions))]
public sealed class NotificationsController(
    INotificationRepository notificationRepository,
    IMapper mapper,
    IUserNotificationRepository userNotificationRepository,
    IUserPlatformRepository userPlatformRepository,
    IUserRepository userRepository,
    IHubContext<NotificationsHub> notificationsHub
) : ControllerBase
{
    private const string UserNotificationCreatedAction = "UserNotificationCreated";

    /// <summary>
    ///     Fetch all the <see cref="Notification" /> entities or all the entities respecting the given criteria.
    /// </summary>
    /// <remarks>
    ///     For example, the following request:
    ///     <c>GET /api/Notifications?ticketId=123e4567-e89b-12d3-a456-426614174000&amp;isRead=true</c>
    ///     Retrieves the entities linked to the ticket with id <b>123e4567-e89b-12d3-a456-426614174000</b> and which have been
    ///     marked as read.
    /// </remarks>
    /// <param name="filters">
    ///     A group of key-value pairs defining the property name and value <see cref="Notification" />
    ///     entities should be filtered by.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the list
    ///     of entities.
    /// </returns>
    /// <response code="200">The entities were found.</response>
    /// <response code="400">The provided filters were not formatted correctly.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpGet]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications(
        [FromQuery] Dictionary<string, string>? filters)
    {
        var notifications = await (filters != null
            ? notificationRepository.GetNotificationsByAsync(filters)
            : notificationRepository.GetNotificationsAsync());

        return Ok(notifications.Select(mapper.Map<NotificationResponseDto>));
    }

    /// <summary>
    ///     Fetch a specific <see cref="Notification" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="Notification" /> entity to fetch.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the
    ///     requested entity.
    /// </returns>
    /// <response code="200">The entity was found.</response>
    /// <response code="400">The provided id was not a Guid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity with the given id didn't exist.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policy.AdminOnly)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<NotificationResponseDto>> GetNotification(Guid id)
    {
        // Check if the requested entity exists.
        if (await notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(mapper.Map<NotificationResponseDto>(notification));
    }

    /// <summary>
    ///     Update a specific <see cref="Notification" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "SendDateTime": "2025-04-27T16:31:17.512Z",
    ///     "Message": "Example message.",
    ///     "IsRead": true,
    ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;NotificationRequestDto&gt;
    ///     &lt;SendDateTime&gt;2025-04-27T16:31:17.512Z&lt;/SendDateTime&gt;
    ///     &lt;Message&gt;Example message.&lt;/Message&gt;
    ///     &lt;IsRead&gt;true&lt;/IsRead&gt;
    ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
    /// &lt;/NotificationRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="id">Identifier of the <see cref="Notification" /> entity to update.</param>
    /// <param name="notificationDto">Object containing the new values the fields should be updated to.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the new or
    ///     updated entity.
    /// </returns>
    /// <response code="201">The entity didn't exist, it was created.</response>
    /// <response code="204">The entity existed, it was updated.</response>
    /// <response code="400">The entity was malformed or the provided id was not a Guid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity was deleted before the update.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    /// <response code="409">The entity was updated by another request at the same time.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policy.AdminOnly)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<NotificationResponseDto>> PutNotification(Guid id,
        NotificationRequestDto notificationDto)
    {
        // If the requested entity does not exist, create a new one.
        if (await notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            return await PostNotification(notificationDto);

        try
        {
            // Update the existing entity with the new data.
            await notificationRepository.UpdateNotificationAsync(mapper.Map(notificationDto, notification));
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the entity is not found in the data source, it was deleted by another user while updating.
            if (await notificationRepository.GetNotificationByIdAsync(id) is null) return NotFound();

            // If the entity is found, it was modified by another at the same time of the update.
            return Conflict();
        }

        // Send a signal to the people who have received this notification and are connected to this hub.
        await SendSignalToClientsAsync("NotificationUpdated", notification.Id);

        return NoContent();
    }

    /// <summary>
    ///     Create a new <see cref="Edit" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "SendDateTime": "2025-04-27T16:31:17.512Z",
    ///     "Message": "Example message.",
    ///     "IsRead": true,
    ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;NotificationRequestDto&gt;
    ///     &lt;SendDateTime&gt;2025-04-27T16:31:17.512Z&lt;/SendDateTime&gt;
    ///     &lt;Message&gt;Example message.&lt;/Message&gt;
    ///     &lt;IsRead&gt;true&lt;/IsRead&gt;
    ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
    /// &lt;/NotificationRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="notificationDto">Object containing the values the new entity should have.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the new
    ///     entity.
    /// </returns>
    /// <response code="201">The entity was created.</response>
    /// <response code="400">The entity was malformed.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpPost]
    [Authorize(Policy = Policy.AdminOnly)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<NotificationResponseDto>> PostNotification(NotificationRequestDto notificationDto)
    {
        var notification = mapper.Map<Notification>(notificationDto);

        // Insert the DTO info into a new entity and add it to the data source.
        await notificationRepository.AddNotificationAsync(notification);

        // Return the created entity and its location to the client.
        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id },
            mapper.Map<NotificationResponseDto>(notification));
    }

    /// <summary>
    ///     Delete a specific <see cref="Edit" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="Edit" /> entity to delete.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="IActionResult" />, which wraps the server response.</returns>
    /// <response code="204">The entity was deleted.</response>
    /// <response code="400">The provided id was not a Guid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity with the given id didn't exist.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policy.AdminOnly)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        // Check if the requested entity exists.
        if (await notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            return NotFound();

        await notificationRepository.DeleteNotificationAsync(notification);

        // Send a signal to the people have received this notification and are connected to this hub.
        await SendSignalToClientsAsync("NotificationDeleted", notification.Id);

        return NoContent();
    }

    internal async Task<Guid?> CreateCreationNotificationsAsync(Ticket ticket)
    {
        // User has created a new ticket; create a notification with the creator as responsible.
        var newTicketCreatedNotification = await CreateNotification(ticket.Id, Notifications.TicketCreated, ticket.CreatorUserId);

        // Find users who have the ticket platform as their preferred platform.
        var operatorsWithPreferredPlatform = await userPlatformRepository.GetUserPlatformsByAsync(
            new Dictionary<string, string> { { nameof(UserPlatform.PlatformId), ticket.PlatformId.ToString() } }
        );

        // If there is any user who prefer this platform, notify the first one with the least assigned tickets.
        if (operatorsWithPreferredPlatform.Any())
        {
            var operatorsSortedByWorkload = from userPlatform in operatorsWithPreferredPlatform
                                            join user in await userRepository.GetUsersAsync()
                                                on userPlatform.UserId equals user.Id
                                            orderby user.AssignedTickets.Count
                                            select user.Id;

            var operatorWithLeastWorkload = operatorsSortedByWorkload.FirstOrDefault();

            // If there is at least one operator, proceed.
            if (operatorWithLeastWorkload != Guid.Empty)
            {
                // Notify the selected operator that the user has created the ticket.
                _ = CreateUserNotification(newTicketCreatedNotification.Id, operatorWithLeastWorkload, UserNotificationCreatedAction);

                // The system will be assigning the ticket to the operator; create a notification with the operator as responsible.
                var systemAssignmentNotificationForOperator = await CreateNotification(ticket.Id, Notifications.TicketAssignedToYouBySystem, operatorWithLeastWorkload);

                // Notify the selected operator that they have been assigned to the ticket.
                _ = CreateUserNotification(systemAssignmentNotificationForOperator.Id, operatorWithLeastWorkload, UserNotificationCreatedAction);

                // The system has assigned the ticket to the operator; create a notification with the operator as responsible.
                var ticketAssignmentNotificationForCreator = await CreateNotification(ticket.Id, Notifications.TicketAssigned, operatorWithLeastWorkload);

                // Notify the ticket creator that the ticket has been assigned to an operator.
                _ = CreateUserNotification(ticketAssignmentNotificationForCreator.Id, ticket.CreatorUserId, UserNotificationCreatedAction);

                return operatorWithLeastWorkload;
            }
        }

        // If there is no user who prefers this platform, notify every operator of the new ticket.
        await SendNotificationToAllOperators(newTicketCreatedNotification.Id);
        return null;
    }

    internal async Task CreateEditNotificationsAsync(Ticket ticket, Guid editor)
    {
        // The user or operator has edited the ticket; create a notification with the editor as responsible.
        var ticketEditedByUserNotification = await CreateNotification(ticket.Id, Notifications.TicketEdited, editor);

        // Check who requested the ticket editing.
        if (editor == ticket.CreatorUserId)
        {
            // If the ticket has been edited by the creator, notify the assigned operator (if any) or all operators.
            if (ticket.OperatorUserId.HasValue)
            {
                // Notify the assigned operator that the ticket has been edited by the creator.
                _ = CreateUserNotification(ticketEditedByUserNotification.Id, ticket.OperatorUserId.Value, UserNotificationCreatedAction);
            }
            else
            {
                // Notify all the operators that the ticket has been edited by the creator.
                await SendNotificationToAllOperators(ticketEditedByUserNotification.Id);
            }
        }
        else if (editor == ticket.OperatorUserId)
        {
            // If the ticket has been edited by the operator, notify the creator.
            _ = CreateUserNotification(ticketEditedByUserNotification.Id, ticket.CreatorUserId, UserNotificationCreatedAction);
        }
        else
        {
            // If the ticket has been edited by someone else (admin), notify both the creator and the operator(s).
            _ = CreateUserNotification(ticketEditedByUserNotification.Id, ticket.CreatorUserId, UserNotificationCreatedAction);

            // If the ticket has an assigned operator, notify them; otherwise notify all operators.
            if (ticket.OperatorUserId.HasValue)
            {
                _ = CreateUserNotification(ticketEditedByUserNotification.Id, ticket.OperatorUserId.Value, UserNotificationCreatedAction);
            }
            else
            {
                await SendNotificationToAllOperators(ticketEditedByUserNotification.Id);
            }
        }
    }

    internal async Task CreateOperatorEditNotificationsAsync(Ticket ticket, Guid? oldAssignedOperator, Guid editor)
    {
        // Check if the operator has been unassigned or assigned/changed.
        if (ticket.OperatorUserId is null)
        {
            // The operator has been unassigned; create a notification with the editor as responsible.
            var ticketUnassignmentNotificationByEditor = await CreateNotification(ticket.Id, Notifications.TicketUnassigned, editor);

            // Notify the ticket creator that the operator has been unassigned.
            _ = CreateUserNotification(ticketUnassignmentNotificationByEditor.Id, ticket.CreatorUserId, UserNotificationCreatedAction);

            // Notify all the operators that the ticket has been unassigned.
            await SendNotificationToAllOperators(ticketUnassignmentNotificationByEditor.Id);
        }
        else
        {
            // The operator has been assigned or changed; create a notification with new operator as responsible to inform them.
            var ticketAssignmentNotificationForNewOperator = await CreateNotification(ticket.Id, Notifications.TicketAssignedToYou, ticket.OperatorUserId.Value);

            // Notify the new operator that they have been assigned to the ticket.
            _ = CreateUserNotification(ticketAssignmentNotificationForNewOperator.Id, ticket.OperatorUserId.Value, UserNotificationCreatedAction);

            // The operator has been assigned or changed; create a notification with new operator as responsible to inform the creator and the old operator (if any).
            var ticketAssignmentNotificationForCreatorAndOldOperator = await CreateNotification(ticket.Id, Notifications.TicketAssigned, ticket.OperatorUserId.Value);

            // Notify the ticket creator that the operator has been assigned or changed.
            _ = CreateUserNotification(ticketAssignmentNotificationForCreatorAndOldOperator.Id, ticket.CreatorUserId, UserNotificationCreatedAction);

            if (oldAssignedOperator.HasValue)
            {
                // Notify the old operator that they have been unassigned from the ticket.
                _ = CreateUserNotification(ticketAssignmentNotificationForCreatorAndOldOperator.Id, oldAssignedOperator.Value, UserNotificationCreatedAction);
            }
        }
    }

    private async Task SendNotificationToAllOperators(Guid notificationId)
    {
        var operators = from user in await userRepository.GetUsersAsync()
                        where user.Type == UserType.Admin || user.Type == UserType.Operator
                        select user.Id;

        foreach (var userId in operators)
        {
            // Notify the operator(s).
            _ = CreateUserNotification(notificationId, userId, UserNotificationCreatedAction);
        }
    }

    private async Task<Notification> CreateNotification(Guid ticketId, string message, Guid userId)
    {
        var notification = new Notification
        {
            TicketId = ticketId,
            Message = message,
            SendDateTime = DateTimeOffset.UtcNow,
            IsRead = false,
            UserId = userId
        };

        await notificationRepository.AddNotificationAsync(notification);

        return notification;
    }

    private async Task<UserNotification> CreateUserNotification(Guid notificationId, Guid receiverUserId, string action)
    {
        var userNotification = new UserNotification
        {
            NotificationId = notificationId,
            ReceiverUserId = receiverUserId
        };

        await userNotificationRepository.AddUserNotificationAsync(userNotification);

        // Notify the user that they have received a new notification.
        await SendSignalToClientAsync(action, userNotification.ReceiverUserId);

        return userNotification;
    }

    private async Task SendSignalToClientsAsync(string action, Guid notificationId)
    {
        var userNotifications = await userNotificationRepository.GetUserNotificationsByAsync(
            new Dictionary<string, string> { { nameof(UserNotification.NotificationId), notificationId.ToString() } }
        );

        foreach (var userNotification in userNotifications)
        {
            await notificationsHub.Clients.Group($"user_{userNotification.ReceiverUserId}").SendAsync(action);
        }
    }

    private async Task SendSignalToClientAsync(string action, Guid receiverId)
    {
        await notificationsHub.Clients.Group($"user_{receiverId}").SendAsync(action);
    }
}