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
///     Web API controller managing requests involving <see cref="UserNotification" /> etities.
/// </summary>
/// <param name="userNotificationRepository">Object defining the operations allowed on the entity type.</param>
/// <param name="mapper">
///     Object definining the mappings of fields between the <see cref="UserNotification" /> entity and
///     its <see cref="UserNotificationRequestDto" /> or <see cref="UserNotificationResponseDto" /> correspondant.
/// </param>
/// <param name="notificationsHub">SignalR hub managing the real-time updates of notifications.</param>
[Route($"v{Service.APIVersion}/{Controller.UserNotifications}")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[FormatFilter]
[ApiConventionType(typeof(ApiConventions))]
public sealed class UserNotificationsController(
    IUserNotificationRepository userNotificationRepository,
    IMapper mapper,
    IHubContext<NotificationsHub> notificationsHub
) : ControllerBase
{
    /// <summary>
    ///     Fetch all the <see cref="UserNotification" /> entities or all the entities respecting the given criteria.
    /// </summary>
    /// <remarks>
    ///     For example, the following request:
    ///     <c>GET /api/UserNotifications?userId=123e4567-e89b-12d3-a456-426614174000</c>
    ///     Retrieves the entities linked to the user with id <b>123e4567-e89b-12d3-a456-426614174000</b>.
    /// </remarks>
    /// <param name="filters">
    ///     A group of key-value pairs defining the property name and value <see cref="UserNotification" />
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
    [Authorize(Policy.Everyone)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<IEnumerable<UserNotificationResponseDto>>> GetUserNotifications(
        [FromQuery] Dictionary<string, string>? filters)
    {
        var userNotifications = await (filters != null
            ? userNotificationRepository.GetUserNotificationsByAsync(filters)
            : userNotificationRepository.GetUserNotificationsAsync());

        return Ok(userNotifications.Select(mapper.Map<UserNotificationResponseDto>));
    }

    /// <summary>
    ///     Fetch a specific <see cref="UserNotification" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="UserNotification" /> entity to fetch.</param>
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
    [Authorize(Policy.AdminOnly)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<UserNotificationResponseDto>> GetUserNotification(Guid id)
    {
        // Check if the requested entity exists.
        if (await userNotificationRepository.GetUserNotificationByIdAsync(id) is not UserNotification userNotification)
            return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(mapper.Map<UserNotificationResponseDto>(userNotification));
    }

    /// <summary>
    ///     Update a specific <see cref="UserNotification" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "UserId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "PlatformId": "123e4567-e89b-12d3-a456-426614174000"
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;UserNotificationRequestDto&gt;
    ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;PlatformId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
    /// &lt;/UserNotificationRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="id">Identifier of the <see cref="UserNotification" /> entity to update.</param>
    /// <param name="userNotificationDto">Object containing the new values the fields should be updated to.</param>
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
    [Authorize(Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<UserNotificationResponseDto>> PutUserNotification(Guid id,
        UserNotificationRequestDto userNotificationDto)
    {
        // If the requested entity does not exist, create a new one.
        if (await userNotificationRepository.GetUserNotificationByIdAsync(id) is not UserNotification userNotification)
            return await PostUserNotification(userNotificationDto);

        try
        {
            // Update the existing entity with the new data.
            await userNotificationRepository.UpdateUserNotificationAsync(mapper.Map(userNotificationDto,
                userNotification));
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the entity is not found in the data source, it was deleted by another user while updating.
            if (await userNotificationRepository.GetUserNotificationByIdAsync(id) is null) return NotFound();

            // If the entity is found, it was modified by another at the same time of the update.
            return Conflict();
        }

        // Send a signal to the people who have received this notification and are connected to this hub.
        await notificationsHub.Clients.Group($"user_{userNotification.ReceiverUserId}").SendAsync("UserNotificationUpdated");

        return NoContent();
    }

    /// <summary>
    ///     Create a new <see cref="Edit" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "UserId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "PlatformId": "123e4567-e89b-12d3-a456-426614174000"
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;UserNotificationRequestDto&gt;
    ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;PlatformId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
    /// &lt;/UserNotificationRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="userNotificationDto">Object containing the values the new entity should have.</param>
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
    [Authorize(Policy.AdminOnly)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<UserNotificationResponseDto>> PostUserNotification(
        UserNotificationRequestDto userNotificationDto)
    {
        var userNotification = mapper.Map<UserNotification>(userNotificationDto);

        // Insert the DTO info into a new entity and add it to the data source.
        await userNotificationRepository.AddUserNotificationAsync(userNotification);

        // Send a signal to the people who received this notification and are connected to this hub.
        await notificationsHub.Clients.Group($"user_{userNotification.ReceiverUserId}").SendAsync("UserNotificationCreated");

        // Return the created entity and its location to the client.
        return CreatedAtAction(nameof(GetUserNotification), new { id = userNotification.Id },
            mapper.Map<UserNotificationResponseDto>(userNotification));
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
    [Authorize(Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<IActionResult> DeleteUserNotification(Guid id)
    {
        // Check if the requested entity exists.
        if (await userNotificationRepository.GetUserNotificationByIdAsync(id) is not UserNotification userNotification)
            return NotFound();

        await userNotificationRepository.DeleteUserNotificationAsync(userNotification);

        // Send a signal to the people who have received this notification and are connected to this hub.
        await notificationsHub.Clients.Group($"user_{userNotification.ReceiverUserId}").SendAsync("UserNotificationDeleted");

        return NoContent();
    }
}