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

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving <see cref="Platform"/> etities.
    /// </summary>
    /// <param name="platformRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Platform"/> entity and its <see cref="PlatformRequestDto"/> or <see cref="PlatformResponseDto"/> correspondant.</param>
    [Route($"v{Service.APIVersion}/Platforms")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ADAticketsApiConventions))]
    [Authorize(Policy = Policy.AdminOnly)]
    public sealed class PlatformsController(IPlatformRepository platformRepository, IMapper mapper) : ControllerBase
    {
        private readonly IPlatformRepository _platformRepository = platformRepository;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Fetch all the <see cref="Platform"/> entities or all the entities respecting the given criteria.
        /// </summary>
        /// <remarks>
        /// For example, the following request:
        /// <c>GET /api/Platforms?name=example&amp;repositoryUrl=github.com</c>
        /// Retrieves the entities containing <b>example</b> in their name and <b>github.com</b> in their repositoryUrl.
        /// </remarks>
        /// <param name="filters">A group of key-value pairs defining the property name and value <see cref="Platform"/> entities should be filtered by.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the list of entities.</returns>
        /// <response code="200">The entities were found.</response>
        /// <response code="400">The provided filters were not formatted correctly.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<IEnumerable<PlatformResponseDto>>> GetPlatforms([FromQuery] IEnumerable<KeyValuePair<string, string>>? filters)
        {
            var platforms = await (filters != null ? _platformRepository.GetPlatformsByAsync(filters) : _platformRepository.GetPlatformsAsync());

            return Ok(platforms.Select(platform => _mapper.Map(platform, new PlatformResponseDto())));
        }

        /// <summary>
        /// Fetch a specific <see cref="Platform"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Platform"/> entity to fetch.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the requested entity.</returns>
        /// <response code="200">The entity was found.</response>
        /// <response code="400">The provided id was not a Guid.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="404">The entity with the given id didn't exist.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet("{id}")]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<PlatformResponseDto>> GetPlatform(Guid id)
        {
            // Check if the requested entity exists.
            if (await _platformRepository.GetPlatformByIdAsync(id) is not Platform platform)
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(_mapper.Map(platform, new PlatformResponseDto()));
        }

        /// <summary>
        /// Update a specific <see cref="Platform"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "Name": "Example",
        ///     "RepositoryUrl": "https://example.com",
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;PlatformRequestDto&gt;
        ///     &lt;Name&gt;Example&lt;/Name&gt;
        ///     &lt;RepositoryUrl&gt;https://example.com&lt;/RepositoryUrl&gt;
        /// &lt;/PlatformRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="id">Identifier of the <see cref="Platform"/> entity to update.</param>
        /// <param name="platformDto">Object containing the new values the fields should be updated to.</param>
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
        [RequiredScope(Scope.Read, Scope.Write)]
        public async Task<ActionResult<PlatformResponseDto>> PutPlatform(Guid id, PlatformRequestDto platformDto)
        {
            // If the requested entity does not exist, create a new one.
            if (await _platformRepository.GetPlatformByIdAsync(id) is not Platform platform)
            {
                return await PostPlatform(platformDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await _platformRepository.UpdatePlatformAsync(_mapper.Map(platformDto, platform));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await _platformRepository.GetPlatformByIdAsync(id) is null)
                {
                    return NotFound();
                }

                // If the entity is found, it was modified by another at the same time of the update.
                return Conflict();
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new <see cref="Platform"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "Name": "Example",
        ///     "RepositoryUrl": "https://example.com",
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;PlatformRequestDto&gt;
        ///     &lt;Name&gt;Example&lt;/Name&gt;
        ///     &lt;RepositoryUrl&gt;https://example.com&lt;/RepositoryUrl&gt;
        /// &lt;/PlatformRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="platformDto">Object containing the values the new entity should have.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new entity.</returns>
        /// <response code="201">The entity was created.</response>
        /// <response code="400">The entity was malformed.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpPost]
        [RequiredScope(Scope.Read, Scope.Write)]
        public async Task<ActionResult<PlatformResponseDto>> PostPlatform(PlatformRequestDto platformDto)
        {
            var platform = _mapper.Map(platformDto, new Platform());

            // Insert the DTO info into a new entity and add it to the data source.
            await _platformRepository.AddPlatformAsync(platform);

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetPlatform), new { id = platform.Id }, platform);
        }

        /// <summary>
        /// Delete a specific <see cref="Platform"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Platform"/> entity to delete.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="IActionResult"/>, which wraps the server response.</returns>
        /// <response code="204">The entity was deleted.</response>
        /// <response code="400">The provided id was not a Guid.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="404">The entity with the given id didn't exist.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpDelete("{id}")]
        [RequiredScope(Scope.Read, Scope.Write)]
        public async Task<IActionResult> DeletePlatform(Guid id)
        {
            // Check if the requested entity exists.
            if (await _platformRepository.GetPlatformByIdAsync(id) is not Platform platform)
            {
                return NotFound();
            }

            await _platformRepository.DeletePlatformAsync(platform);

            return NoContent();
        }
    }
}
