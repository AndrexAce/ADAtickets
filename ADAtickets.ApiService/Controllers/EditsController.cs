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

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving <see cref="Edit"/> etities.
    /// </summary>
    /// <param name="editRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Edit"/> entity and its <see cref="EditRequestDto"/> or <see cref="EditResponseDto"/> correspondant.</param>
    [Route($"v{Service.APIVersion}/{Controller.Edits}")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ApiConventions))]
    public sealed class EditsController(IEditRepository editRepository, IMapper mapper) : ControllerBase
    {
        /// <summary>
        /// Fetch all the <see cref="Edit"/> entities or all the entities respecting the given criteria.
        /// </summary>
        /// <remarks>
        /// For example, the following request:
        /// <c>GET /api/Edits?ticketId=123e4567-e89b-12d3-a456-426614174000&amp;oldStatus=unassigned</c>
        /// Retrieves the entities linked to the ticket with id <b>123e4567-e89b-12d3-a456-426614174000</b> and which were, before the edit, in the <b>Unassigned</b> status.
        /// </remarks>
        /// <param name="filters">A group of key-value pairs defining the property name and value <see cref="Edit"/> entities should be filtered by.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the list of entities.</returns>
        /// <response code="200">The entities were found.</response>
        /// <response code="400">The provided filters were not formatted correctly.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet]
        [Authorize(Policy = Policy.Everyone)]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<IEnumerable<EditResponseDto>>> GetEdits([FromQuery] Dictionary<string, string>? filters)
        {
            IEnumerable<Edit> edits = await (filters != null ? editRepository.GetEditsByAsync(filters) : editRepository.GetEditsAsync());

            return Ok(edits.Select(mapper.Map<EditResponseDto>));
        }

        /// <summary>
        /// Fetch a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to fetch.</param>
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
        public async Task<ActionResult<EditResponseDto>> GetEdit(Guid id)
        {
            // Check if the requested entity exists.
            if (await editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(mapper.Map<EditResponseDto>(edit));
        }

        /// <summary>
        /// Update a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <remarks>
        /// JSON request body example:
        /// <code>
        /// {
        ///     "EditDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Description": "Example description.",
        ///     "OldStatus": "Unassigned",
        ///     "NewStatus": "WaitingOperator",
        ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;EditRequestDto&gt;
        ///     &lt;EditDateTime&gt;2025-04-27T16:31:17.512Z&lt;/EditDateTime&gt;
        ///     &lt;Description&gt;Example description.&lt;/Description&gt;
        ///     &lt;OldStatus&gt;Unassigned&lt;/OldStatus&gt;
        ///     &lt;NewStatus&gt;WaitingOperator&lt;/NewStatus&gt;
        ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
        ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
        /// &lt;/EditRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to update.</param>
        /// <param name="editDto">Object containing the new values the fields should be updated to.</param>
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
        public async Task<ActionResult<EditResponseDto>> PutEdit(Guid id, EditRequestDto editDto)
        {
            // If the requested entity does not exist, create a new one.
            if (await editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return await PostEdit(editDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await editRepository.UpdateEditAsync(mapper.Map(editDto, edit));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await editRepository.GetEditByIdAsync(id) is null)
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
        ///     "EditDateTime": "2025-04-27T16:31:17.512Z",
        ///     "Description": "Example description.",
        ///     "OldStatus": "Unassigned",
        ///     "NewStatus": "WaitingOperator",
        ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
        ///     "UserId": "123e4567-e89b-12d3-a456-426614174000"
        /// }
        /// </code>
        /// XML request body example:
        /// <code>
        /// &lt;EditRequestDto&gt;
        ///     &lt;EditDateTime&gt;2025-04-27T16:31:17.512Z&lt;/EditDateTime&gt;
        ///     &lt;Description&gt;Example description.&lt;/Description&gt;
        ///     &lt;OldStatus&gt;Unassigned&lt;/OldStatus&gt;
        ///     &lt;NewStatus&gt;WaitingOperator&lt;/NewStatus&gt;
        ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
        ///     &lt;UserId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/UserId&gt;
        /// &lt;/EditRequestDto&gt;
        /// </code>
        /// </remarks>
        /// <param name="editDto">Object containing the values the new entity should have.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new entity.</returns>
        /// <response code="201">The entity was created.</response>
        /// <response code="400">The entity was malformed.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpPost]
        [Authorize(Policy = Policy.AdminOnly)]
        [RequiredScope(Scope.Read, Scope.Write)]
        public async Task<ActionResult<EditResponseDto>> PostEdit(EditRequestDto editDto)
        {
            Edit edit = mapper.Map<Edit>(editDto);

            // Insert the DTO info into a new entity and add it to the data source.
            await editRepository.AddEditAsync(edit);

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetEdit), new { id = edit.Id }, mapper.Map<EditResponseDto>(edit));
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
        public async Task<IActionResult> DeleteEdit(Guid id)
        {
            // Check if the requested entity exists.
            if (await editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return NotFound();
            }

            await editRepository.DeleteEditAsync(edit);

            return NoContent();
        }

        internal async Task CreateCreationEntriesAsync(Ticket ticket, Guid? chosenOperatorId)
        {
            // Create the edit about the ticket creation by the creator.
            Edit edit = new()
            {
                EditDateTime = DateTime.UtcNow,
                Description = Edits.TicketCreated,
                OldStatus = Status.Unassigned,
                NewStatus = Status.Unassigned,
                TicketId = ticket.Id,
                UserId = ticket.CreatorUserId
            };

            await editRepository.AddEditAsync(edit);

            // If there is an automatically assigned operator, create the edit of the ticket assignment to the operator too.
            if (chosenOperatorId.HasValue)
            {
                Edit assignmentEdit = new()
                {
                    EditDateTime = DateTime.UtcNow,
                    Description = Edits.TicketAutoAssigned,
                    OldStatus = Status.Unassigned,
                    NewStatus = Status.WaitingOperator,
                    TicketId = ticket.Id,
                    UserId = chosenOperatorId.Value
                };

                await editRepository.AddEditAsync(assignmentEdit);
            }
        }

        internal async Task CreateEditEntryAsync(Ticket ticket, Guid editor)
        {
            // Create the edit about the ticket edit by the editor.
            Edit edit = new()
            {
                EditDateTime = DateTime.UtcNow,
                Description = Edits.TicketEdited,
                OldStatus = ticket.Status,
                NewStatus = ticket.Status,
                TicketId = ticket.Id,
                UserId = editor
            };

            await editRepository.AddEditAsync(edit);
        }

        internal async Task CreateOperatorEditEntryAsync(Ticket ticket, Guid? oldAssignedOperator, Guid editor)
        {
            Edit edit;

            if (ticket.OperatorUserId is null)
            {
                // If the ticket is unassigned, switch to the unassigned status.
                edit = new Edit
                {
                    EditDateTime = DateTime.UtcNow,
                    Description = Edits.TicketUnassigned,
                    OldStatus = ticket.Status,
                    NewStatus = Status.Unassigned,
                    TicketId = ticket.Id,
                    UserId = editor
                };
            }
            else
            {
                // If the ticket is assigned to another operator, switch to waiting if it was unassigned before, otherwise keep the current status.
                edit = new Edit
                {
                    EditDateTime = DateTime.UtcNow,
                    Description = Edits.TicketAssigned,
                    OldStatus = ticket.Status,
                    NewStatus = oldAssignedOperator.HasValue ? ticket.Status : Status.WaitingOperator,
                    TicketId = ticket.Id,
                    UserId = ticket.OperatorUserId.Value
                };
            }

            await editRepository.AddEditAsync(edit);
        }
    }
}
