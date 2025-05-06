/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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
using ADAtickets.ApiService.Dtos.Requests;
using ADAtickets.ApiService.Dtos.Responses;
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving <see cref="Notification"/> etities.
    /// </summary>
    /// <param name="notificationRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Notification"/> entity and its <see cref="NotificationRequestDto"/> or <see cref="NotificationResponseDto"/> correspondant.</param>
    [Route("api/Notifications")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ADAticketsApiConventions))]
    [AutoValidateAntiforgeryToken]
    public sealed class NotificationsController(INotificationRepository notificationRepository, IMapper mapper) : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly IMapper _mapper = mapper;

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
        [Authorize(Policy = "AuthenticatedEveryone")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications([FromQuery] IEnumerable<KeyValuePair<string, string>>? filters)
        {
            var notifications = await (filters != null ? _notificationRepository.GetNotificationsByAsync(filters) : _notificationRepository.GetNotificationsAsync());

            return Ok(notifications.Select(notification => _mapper.Map(notification, new NotificationResponseDto())));
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
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<NotificationResponseDto>> GetNotification(Guid id)
        {
            // Check if the requested entity exists.
            if (await _notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(_mapper.Map(notification, new NotificationResponseDto()));
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
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<NotificationResponseDto>> PutNotification(Guid id, NotificationRequestDto notificationDto)
        {
            // If the requested entity does not exist, create a new one.
            if (await _notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            {
                return await PostNotification(notificationDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await _notificationRepository.UpdateNotificationAsync(_mapper.Map(notificationDto, notification));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await _notificationRepository.GetNotificationByIdAsync(id) is null)
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<NotificationResponseDto>> PostNotification(NotificationRequestDto notificationDto)
        {
            var notification = _mapper.Map(notificationDto, new Notification());

            // Insert the DTO info into a new entity and add it to the data source.
            await _notificationRepository.AddNotificationAsync(notification);

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
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
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            // Check if the requested entity exists.
            if (await _notificationRepository.GetNotificationByIdAsync(id) is not Notification notification)
            {
                return NotFound();
            }

            await _notificationRepository.DeleteNotificationAsync(notification);

            return NoContent();
        }
    }
}
