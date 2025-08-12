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
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Web.Resource;
using System.Net.Mime;
using Controller = ADAtickets.Shared.Constants.Controller;

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving <see cref="Notification"/> etities.
    /// </summary>
    /// <param name="notificationRepository">Object defining the operations allowed on the <see cref="Notification"/> entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Notification"/> entity and its <see cref="NotificationRequestDto"/> or <see cref="NotificationResponseDto"/> correspondant.</param>
    /// <param name="userNotificationRepository">Object defining the operations allowed on the <see cref="UserNotification"/> entity type.</param>
    /// <param name="userPlatformRepository">Object defining the operations allowed on the <see cref="UserPlatform"/> entity type.</param>
    /// <param name="userRepository">Object defining the operations allowed on the <see cref="User"/> entity type.</param>
    /// <param name="stringLocalizer">Object used to translate strings.</param>
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
        IStringLocalizer<NotificationsController> stringLocalizer
        ) : ControllerBase
    {
        /// <summary>
        /// Fetch all the <see cref="Notification"/> entities or all the entities respecting the given criteria.
        /// </summary>
        /// <remarks>
        /// For example, the following request:
        /// <c>GET /api/Notifications?ticketId=123e4567-e89b-12d3-a456-426614174000&amp;isRead=true</c>
        /// Retrieves the entities linked to the ticket with id <b>123e4567-e89b-12d3-a456-426614174000</b> and which have been marked as read.
        /// </remarks>
        /// <param name="filters">A group of key-value pairs defining the property name and value <see cref="Notification"/> entities should be filtered by.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the list of entities.</returns>
        /// <response code="200">The entities were found.</response>
        /// <response code="400">The provided filters were not formatted correctly.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet]
        [Authorize(Policy = Policy.Everyone)]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications([FromQuery] Dictionary<string, string>? filters)
        {
            IEnumerable<Notification> notifications = await (filters != null ? notificationRepository.GetNotificationsByAsync(filters) : notificationRepository.GetNotificationsAsync());

            return Ok(notifications.Select(mapper.Map<NotificationResponseDto>));
        }

        /// <summary>
        /// Fetch a specific <see cref="Notification"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Notification"/> entity to fetch.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the requested entity.</returns>
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
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(mapper.Map<NotificationResponseDto>(notification));
        }

        /// <summary>
        /// Update a specific <see cref="Notification"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "SendDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Message": "Example message.",
        ///     "IsRead": true,
        ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;NotificationRequestDto&gt;
        ///     &lt;SendDateTime&gt;2025-04-27T16:31:17.512Z&lt;/SendDateTime&gt;
        ///     &lt;Message&gt;Example message.&lt;/Message&gt;
        ///     &lt;IsRead&gt;true&lt;/IsRead&gt;
        ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
        ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
        /// &lt;/NotificationRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="id">Identifier of the <see cref="Notification"/> entity to update.</param>
        /// <param name="notificationDto">Object containing the new values the fields should be updated to.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new or updated entity.</returns>
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
        public async Task<ActionResult<NotificationResponseDto>> PutNotification(Guid id, NotificationRequestDto notificationDto)
        {
            // If the requested entity does not exist, create a new one.
            if (await notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            {
                return await PostNotification(notificationDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await notificationRepository.UpdateNotificationAsync(mapper.Map(notificationDto, notification));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await notificationRepository.GetNotificationByIdAsync(id) is null)
                {
                    return NotFound();
                }

                // If the entity is found, it was modified by another at the same time of the update.
                return Conflict();
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new <see cref="Edit"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "SendDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Message": "Example message.",
        ///     "IsRead": true,
        ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
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
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new entity.</returns>
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
            Notification notification = mapper.Map<Notification>(notificationDto);

            // Insert the DTO info into a new entity and add it to the data source.
            await notificationRepository.AddNotificationAsync(notification);

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, mapper.Map<NotificationResponseDto>(notification));
        }

        /// <summary>
        /// Delete a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to delete.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="IActionResult"/>, which wraps the server response.</returns>
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
            {
                return NotFound();
            }

            await notificationRepository.DeleteNotificationAsync(notification);

            return NoContent();
        }

        internal async Task<Guid?> CreateNotificationsAsync(Ticket ticket)
        {
            // Create the creation notification
            var ticketCreatedNotification = CreateNotification(ticket.Id, stringLocalizer["TicketCreatedNotification"], ticket.CreatorUserId);
            await notificationRepository.AddNotificationAsync(ticketCreatedNotification);

            // Find users who have the ticket platform as their preferred platform
            var userPlatforms = await userPlatformRepository.GetUserPlatformsByAsync(
            [
                new KeyValuePair<string, string>(nameof(UserPlatform.PlatformId), ticket.PlatformId.ToString())
            ]);

            // If there is any user who prefer this platform, notify the first one with the least assigned tickets or the only one if that is the case.
            if (userPlatforms.Any())
            {
                var sortedOperators = from userPlatform in userPlatforms
                                      join user in await userRepository.GetUsersAsync()
                                      on userPlatform.UserId equals user.Id
                                      orderby user.AssignedTickets.Count ascending
                                      select user.Id;

                // Create the creation notification link
                var userNotificationCreation = CreateUserNotification(ticketCreatedNotification.Id, sortedOperators.FirstOrDefault());
                await userNotificationRepository.AddUserNotificationAsync(userNotificationCreation);

                // Create the assignment notification for the operator
                var ticketAssignedNotificationOperator = CreateNotification(ticket.Id, stringLocalizer["TicketAssignedToYouNotification"], sortedOperators.FirstOrDefault());
                await notificationRepository.AddNotificationAsync(ticketAssignedNotificationOperator);

                // Create the assignment notification link for the operator
                var userNotificationAssignmentOperator = CreateUserNotification(ticketAssignedNotificationOperator.Id, sortedOperators.FirstOrDefault());
                await userNotificationRepository.AddUserNotificationAsync(userNotificationAssignmentOperator);

                // Create the assignment notification for the user
                var ticketAssignedNotificationUser = CreateNotification(ticket.Id, stringLocalizer["TicketAssignedNotification"], ticket.CreatorUserId);
                await notificationRepository.AddNotificationAsync(ticketAssignedNotificationUser);

                // Create the assignment notification link for the user
                var userNotificationAssignmentUser = CreateUserNotification(ticketAssignedNotificationUser.Id, ticket.CreatorUserId);
                await userNotificationRepository.AddUserNotificationAsync(userNotificationAssignmentUser);

                return sortedOperators.FirstOrDefault();
            }
            else
            {
                var operators = from user in await userRepository.GetUsersAsync()
                                where user.Type == UserType.Admin || user.Type == UserType.Operator
                                select user.Id;

                // If there is no user who prefers this platform, notify every operator of the new ticket.
                foreach (Guid userId in operators)
                {
                    // Create the creation notification link
                    var userNotificationCreation = CreateUserNotification(ticketCreatedNotification.Id, userId);
                    await userNotificationRepository.AddUserNotificationAsync(userNotificationCreation);
                }

                return null;
            }
        }

        private static Notification CreateNotification(Guid ticketId, string message, Guid userId) => new()
        {
            TicketId = ticketId,
            Message = message,
            SendDateTime = DateTimeOffset.UtcNow,
            IsRead = false,
            UserId = userId
        };

        private static UserNotification CreateUserNotification(Guid notificationId, Guid receiverUserId) => new()
        {
            NotificationId = notificationId,
            ReceiverUserId = receiverUserId
        };
    }
}
