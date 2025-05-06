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
    /// Web API controller managing requests involving <see cref="Ticket"/> etities.
    /// </summary>
    /// <param name="ticketRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Ticket"/> entity and its <see cref="TicketRequestDto"/> or <see cref="TicketResponseDto"/> correspondant.</param>
    [Route("api/Tickets")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ADAticketsApiConventions))]
    [AutoValidateAntiforgeryToken]
    public sealed class TicketsController(ITicketRepository ticketRepository, IMapper mapper) : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository = ticketRepository;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Fetch all the <see cref="Ticket"/> entities or all the entities respecting the given criteria.
        /// </summary>
        /// <remarks>
        /// For example, the following request:
        /// <c>GET /api/Tickets?type=bug&amp;creationDateTime=2025-04-27T16:31:17.512Z</c>
        /// Retrieves the entities of type <b>Bug</b> and created the day <b>2025-04-27</b>.
        /// </remarks>
        /// <param name="filters">A group of key-value pairs defining the property name and value <see cref="Ticket"/> entities should be filtered by.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the list of entities.</returns>
        /// <response code="200">The entities were found.</response>
        /// <response code="400">The provided filters were not formatted correctly.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet]
        [Authorize(Policy = "AuthenticatedEveryone")]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets([FromQuery] IEnumerable<KeyValuePair<string, string>>? filters)
        {
            var tickets = await (filters != null ? _ticketRepository.GetTicketsByAsync(filters) : _ticketRepository.GetTicketsAsync());

            return Ok(tickets.Select(ticket => _mapper.Map(ticket, new TicketResponseDto())));
        }

        /// <summary>
        /// Fetch a specific <see cref="Ticket"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Ticket"/> entity to fetch.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the requested entity.</returns>
        /// <response code="200">The entity was found.</response>
        /// <response code="400">The provided id was not a Guid.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="404">The entity with the given id didn't exist.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet("{id}")]
        [Authorize(Policy = "AuthenticatedEveryone")]
        public async Task<ActionResult<TicketResponseDto>> GetTicket(Guid id)
        {
            // Check if the requested entity exists.
            if (await _ticketRepository.GetTicketByIdAsync(id) is not Ticket ticket)
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(_mapper.Map(ticket, new TicketResponseDto()));
        }

        /// <summary>
        /// Update a specific <see cref="Ticket"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "Type": "Bug",
        ///     "CreationDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Title": "Example title.",
        ///     "Description": "Example description.",
        ///     "Priority": "Low",
        ///     "Status": "Unassigned",
        ///     "WorkItemId": 1,
        ///     "PlatformId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "CreatorUserId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "OperatorUserId": null
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;TicketRequestDto&gt;
        ///     &lt;Type&gt;Bug&lt;/Type&gt;
        ///     &lt;CreationDateTime&gt;2025-04-27T16:31:17.512Z&lt;/CreationDateTime&gt;
        ///     &lt;Title&gt;Example title.&lt;/Title&gt;
        ///     &lt;Description&gt;Example description.&lt;/Description&gt;
        ///     &lt;Priority&gt;Low&lt;/Priority&gt;
        ///     &lt;Status&gt;Unassigned&lt;/Status&gt;
        ///     &lt;WorkItemId&gt;1&lt;/WorkItemId&gt;
        ///     &lt;PlatformId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/PlatformId&gt;
        ///     &lt;CreatorUserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/CreatorUserId&gt;
        /// &lt;/TicketRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="id">Identifier of the <see cref="Ticket"/> entity to update.</param>
        /// <param name="ticketDto">Object containing the new values the fields should be updated to.</param>
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
        [Authorize(Policy = "AuthenticatedEveryone")]
        public async Task<ActionResult<TicketResponseDto>> PutTicket(Guid id, TicketRequestDto ticketDto)
        {
            // If the requested entity does not exist, create a new one.
            if (await _ticketRepository.GetTicketByIdAsync(id) is not Ticket ticket)
            {
                return await PostTicket(ticketDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await _ticketRepository.UpdateTicketAsync(_mapper.Map(ticketDto, ticket));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await _ticketRepository.GetTicketByIdAsync(id) is null)
                {
                    return NotFound();
                }

                // If the entity is found, it was modified by another at the same time of the update.
                return Conflict();
            }

            return NoContent();
        }

        /// <summary>
        /// Create a new <see cref="Ticket"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "Type": "Bug",
        ///     "CreationDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Title": "Example title.",
        ///     "Description": "Example description.",
        ///     "Priority": "Low",
        ///     "Status": "Unassigned",
        ///     "WorkItemId": 1,
        ///     "PlatformId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "CreatorUserId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "OperatorUserId": null
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;TicketRequestDto&gt;
        ///     &lt;Type&gt;Bug&lt;/Type&gt;
        ///     &lt;CreationDateTime&gt;2025-04-27T16:31:17.512Z&lt;/CreationDateTime&gt;
        ///     &lt;Title&gt;Example title.&lt;/Title&gt;
        ///     &lt;Description&gt;Example description.&lt;/Description&gt;
        ///     &lt;Priority&gt;Low&lt;/Priority&gt;
        ///     &lt;Status&gt;Unassigned&lt;/Status&gt;
        ///     &lt;WorkItemId&gt;1&lt;/WorkItemId&gt;
        ///     &lt;PlatformId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/PlatformId&gt;
        ///     &lt;CreatorUserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/CreatorUserId&gt;
        /// &lt;/TicketRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="ticketDto">Object containing the values the new entity should have.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new entity.</returns>
        /// <response code="201">The entity was created.</response>
        /// <response code="400">The entity was malformed.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpPost]
        [Authorize(Policy = "AuthenticatedEveryone")]
        public async Task<ActionResult<TicketResponseDto>> PostTicket(TicketRequestDto ticketDto)
        {
            var ticket = _mapper.Map(ticketDto, new Ticket());

            // Insert the DTO info into a new entity and add it to the data source.
            await _ticketRepository.AddTicketAsync(ticket);

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        /// <summary>
        /// Delete a specific <see cref="Ticket"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Ticket"/> entity to delete.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="IActionResult"/>, which wraps the server response.</returns>
        /// <response code="204">The entity was deleted.</response>
        /// <response code="400">The provided id was not a Guid.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="404">The entity with the given id didn't exist.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AuthenticatedOperator")]
        public async Task<IActionResult> DeleteTicket(Guid id)
        {
            // Check if the requested entity exists.
            if (await _ticketRepository.GetTicketByIdAsync(id) is not Ticket ticket)
            {
                return NotFound();
            }

            await _ticketRepository.DeleteTicketAsync(ticket);

            return NoContent();
        }
    }
}
