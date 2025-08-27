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
using Microsoft.Identity.Web.Resource;
using System.Net.Mime;
using Controller = ADAtickets.Shared.Constants.Controller;

namespace ADAtickets.ApiService.Controllers;

/// <summary>
///     Web API controller managing requests involving <see cref="User" /> etities.
/// </summary>
/// <param name="userRepository">Object defining the operations allowed on the entity type.</param>
/// <param name="mapper">
///     Object definining the mappings of fields between the <see cref="User" /> entity and its
///     <see cref="UserRequestDto" /> or <see cref="UserResponseDto" /> correspondant.
/// </param>
[Route($"v{Service.APIVersion}/{Controller.Users}")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[FormatFilter]
[ApiConventionType(typeof(ApiConventions))]
public sealed class UsersController(IUserRepository userRepository, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Fetch all the <see cref="User" /> entities or all the entities respecting the given criteria.
    /// </summary>
    /// <remarks>
    ///     For example, the following request:
    ///     <c>GET /api/Users?name=john&amp;microsoftAccountId=null&amp;pageNumber=1&amp;pageSize=10</c>
    ///     Retrieves the entities containing <b>john</b> in their name and not linked to a Microsoft account, returning the first page with 10 items.
    /// </remarks>
    /// <param name="pageNumber">The page number to retrieve (optional, defaults to returning all items unpaged).</param>
    /// <param name="pageSize">The number of items per page (optional, required when pageNumber is specified).</param>
    /// <param name="filters">
    ///     A group of key-value pairs defining the property name and value <see cref="User" /> entities
    ///     should be filtered by.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the paginated list
    ///     of entities.
    /// </returns>
    /// <response code="200">The entities were found.</response>
    /// <response code="400">The provided filters were not formatted correctly or pagination parameters are invalid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpGet]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<Page<UserResponseDto>>> GetUsers(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] Dictionary<string, string>? filters = null)
    {
        var users = await (filters != null ? userRepository.GetUsersByAsync(filters) : userRepository.GetUsersAsync());
        var userDtos = users.Select(mapper.Map<UserResponseDto>);

        // Return paginated or unpaginated results based on parameters
        Page<UserResponseDto> result;

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            result = new Page<UserResponseDto>(userDtos, pageNumber.Value, pageSize.Value);
        }
        else
        {
            result = new Page<UserResponseDto>(userDtos);
        }

        return Ok(result);
    }

    /// <summary>
    ///     Fetch a specific <see cref="User" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="User" /> entity to fetch.</param>
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
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        // Check if the requested entity exists.
        if (await userRepository.GetUserByIdAsync(id) is not User user) return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    ///     Fetch a specific <see cref="User" /> entity.
    /// </summary>
    /// <param name="email">Email of the <see cref="User" /> entity to fetch.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the
    ///     requested entity.
    /// </returns>
    /// <response code="200">The entity was found.</response>
    /// <response code="400">The provided email was not valid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity with the given email didn't exist.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpGet("{email}")]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<UserResponseDto>> GetUser(string email)
    {
        // Check if the requested entity exists.
        Dictionary<string, string> emailFilter = new()
        {
            { nameof(UserResponseDto.Email), email }
        };

        if ((await userRepository.GetUsersByAsync(emailFilter)).FirstOrDefault() is not User user) return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    ///     Update a specific <see cref="User" /> entity.
    /// </summary>
    /// <remarks>
    /// JSON request body example:
    /// <code>
    /// {
    ///     "Email": "john.smith@outlook.com",
    ///     "Name": "John",
    ///     "Surname": "Smith",
    ///     "AreEmailNotificationsEnabled": true,
    ///     "Type": "User",
    /// }
    /// </code>
    /// XML request body example:
    /// <code>
    /// &lt;UserRequestDto&gt;
    ///     &lt;Email&gt;john.smith@outlook.com&lt;Email&gt;
    ///     &lt;Name&gt;John&lt;/Name&gt;
    ///     &lt;Surname&gt;Smith&lt;/Surname&gt;
    ///     &lt;AreEmailNotificationsEnabled&gt;true&lt;/AreEmailNotificationsEnabled&gt;
    ///     &lt;Type&gt;User&lt;/Type&gt;
    /// &lt;/UserRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="id">Identifier of the <see cref="User" /> entity to update.</param>
    /// <param name="userDto">Object containing the new values the fields should be updated to.</param>
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
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<UserResponseDto>> PutUser(Guid id, UserRequestDto userDto)
    {
        // If the requested entity does not exist, create a new one.
        if (await userRepository.GetUserByIdAsync(id) is not User user) return await PostUser(userDto);

        try
        {
            // Update the existing entity with the new data.
            await userRepository.UpdateUserAsync(mapper.Map(userDto, user));
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the entity is not found in the data source, it was deleted by another user while updating.
            if (await userRepository.GetUserByIdAsync(id) is null) return NotFound();

            // If the entity is found, it was modified by another at the same time of the update.
            return Conflict();
        }

        return NoContent();
    }

    /// <summary>
    ///     Update a specific <see cref="User" /> entity.
    /// </summary>
    /// <remarks>
    /// JSON request body example:
    /// <code>
    /// {
    ///     "Email": "john.smith@outlook.com",
    ///     "Name": "John",
    ///     "Surname": "Smith",
    ///     "AreEmailNotificationsEnabled": true,
    ///     "Type": "User",
    /// }
    /// </code>
    /// XML request body example:
    /// <code>
    /// &lt;UserRequestDto&gt;
    ///     &lt;Email&gt;john.smith@outlook.com&lt;Email&gt;
    ///     &lt;Name&gt;John&lt;/Name&gt;
    ///     &lt;Surname&gt;Smith&lt;/Surname&gt;
    ///     &lt;AreEmailNotificationsEnabled&gt;true&lt;/AreEmailNotificationsEnabled&gt;
    ///     &lt;Type&gt;User&lt;/Type&gt;
    /// &lt;/UserRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="email">Email of the <see cref="User" /> entity to update.</param>
    /// <param name="userDto">Object containing the new values the fields should be updated to.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the new or
    ///     updated entity.
    /// </returns>
    /// <response code="201">The entity didn't exist, it was created.</response>
    /// <response code="204">The entity existed, it was updated.</response>
    /// <response code="400">The entity was malformed or the provided email was not valid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity was deleted before the update.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    /// <response code="409">The entity was updated by another request at the same time.</response>
    [HttpPut("{email}")]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<UserResponseDto>> PutUser(string email, UserRequestDto userDto)
    {
        // If the requested entity does not exist, create a new one.
        Dictionary<string, string> emailFilter = new()
        {
            { nameof(UserResponseDto.Email), email }
        };

        if ((await userRepository.GetUsersByAsync(emailFilter)).FirstOrDefault() is not User user)
            return await PostUser(userDto);

        try
        {
            // Update the existing entity with the new data.
            await userRepository.UpdateUserAsync(mapper.Map(userDto, user));
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the entity is not found in the data source, it was deleted by another user while updating.
            if (await userRepository.GetUserByIdAsync(user.Id) is null) return NotFound();

            // If the entity is found, it was modified by another at the same time of the update.
            return Conflict();
        }

        return NoContent();
    }

    /// <summary>
    ///     Create a new <see cref="User" /> entity.
    /// </summary>
    /// <remarks>
    /// JSON request body example:
    /// <code>
    /// {
    ///     "Email": "john.smith@outlook.com",
    ///     "Name": "John",
    ///     "Surname": "Smith",
    ///     "AreEmailNotificationsEnabled": true,
    ///     "Type": "User",
    /// }
    /// </code>
    /// XML request body example:
    /// <code>
    /// &lt;UserRequestDto&gt;
    ///     &lt;Email&gt;john.smith@outlook.com&lt;Email&gt;
    ///     &lt;Name&gt;John&lt;/Name&gt;
    ///     &lt;Surname&gt;Smith&lt;/Surname&gt;
    ///     &lt;AreEmailNotificationsEnabled&gt;true&lt;/AreEmailNotificationsEnabled&gt;
    ///     &lt;Type&gt;User&lt;/Type&gt;
    /// &lt;/UserRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="userDto">Object containing the values the new entity should have.</param>
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
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<UserResponseDto>> PostUser(UserRequestDto userDto)
    {
        var user = mapper.Map<User>(userDto);

        // Insert the DTO info into a new entity and add it to the data source.
        await userRepository.AddUserAsync(user);

        // Return the created entity and its location to the client.
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    ///     Delete a specific <see cref="User" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="User" /> entity to delete.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="IActionResult" />, which wraps the server response.</returns>
    /// <response code="204">The entity was deleted.</response>
    /// <response code="400">The provided id was not a Guid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity with the given id didn't exist.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Check if the requested entity exists.
        if (await userRepository.GetUserByIdAsync(id) is not User user) return NotFound();

        await userRepository.DeleteUserAsync(user);

        return NoContent();
    }

    /// <summary>
    ///     Delete a specific <see cref="User" /> entity.
    /// </summary>
    /// <param name="email">Email of the <see cref="User" /> entity to delete.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="IActionResult" />, which wraps the server response.</returns>
    /// <response code="204">The entity was deleted.</response>
    /// <response code="400">The provided email was not valid.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="404">The entity with the given email didn't exist.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpDelete("{email}")]
    [Authorize(Policy = Policy.Everyone)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<IActionResult> DeleteUser(string email)
    {
        // Check if the requested entity exists.
        Dictionary<string, string> emailFilter = new()
        {
            { nameof(UserResponseDto.Email), email }
        };

        if ((await userRepository.GetUsersByAsync(emailFilter)).FirstOrDefault() is not User user) return NotFound();

        await userRepository.DeleteUserAsync(user);

        return NoContent();
    }
}