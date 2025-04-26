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
using ADAtickets.ApiService.Dtos;
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving <see cref="Edit"/> etities.
    /// </summary>
    /// <param name="editRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="mapper">Object definining the mappings of fields between the <see cref="Edit"/> entity and its <see cref="EditDto"/> correspondant.</param>
    [Route("api/Edits")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ADAticketsApiConventions))]
    class EditsController(IEditRepository editRepository, IMapper mapper) : ControllerBase
    {
        private readonly IEditRepository _editRepository = editRepository;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Handles the GET request to fetch all the <see cref="Edit"/> entities.
        /// </summary>
        /// <returns>A response with an asynchronously enumerable sequence of <see cref="EditDto"/> in its body.</returns>
        [HttpGet]
        public async IAsyncEnumerable<EditDto> GetEdits()
        {
            // Iterate all the elements in the sequence.
            await foreach (var edit in _editRepository.GetEdits())
            {
                // Insert the entity data into a new DTO and send it to the client.
                yield return _mapper.Map(edit, new EditDto());
            }
        }

        /// <summary>
        /// Handles the GET request to fetch a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to fetch.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response.</returns>
        [HttpGet("{id}.{format?}")]
        public async Task<ActionResult<EditDto>> GetEdit(Guid id)
        {
            // Check if the requested entity exists.
            if (await _editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(_mapper.Map(edit, new EditDto()));
        }

        /// <summary>
        /// Handles the PUT request to update a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to update.</param>
        /// <param name="editDto">Object containing the new values the fields should be updated to.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new object.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<EditDto>> PutEdit(Guid id, EditDto editDto)
        {
            // Check the consistency of the request.
            if (id != editDto.Id)
            {
                return BadRequest();
            }

            // If the requested entity does not exist, create a new one.
            if (await _editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return await PostEdit(editDto);
            }

            try
            {
                // Update the existing entity with the new data.
                await _editRepository.UpdateEditAsync(_mapper.Map(editDto, edit));
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the entity is not found in the data source, it was deleted by another user while updating.
                if (await _editRepository.GetEditByIdAsync(id) is null)
                {
                    return NotFound();
                }

                // If the entity is found, it was modified by another at the same time of the update.
                return Conflict();
            }

            return NoContent();
        }

        /// <summary>
        /// Handles the POST request to create a new <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="edit">Object containing the values the new entity should have.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the new object.</returns>
        [HttpPost]
        public async Task<ActionResult<EditDto>> PostEdit(EditDto edit)
        {
            // Insert the DTO info into a new entity and add it to the data source.
            await _editRepository.AddEditAsync(_mapper.Map(edit, new Edit()));

            // Return the created entity and its location to the client.
            return CreatedAtAction(nameof(GetEdit), new { id = edit.Id }, edit);
        }

        /// <summary>
        /// Handles the DELETE request to delete a specific <see cref="Edit"/> entity.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="Edit"/> entity to delete.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="IActionResult"/>, which wraps the server response.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEdit(Guid id)
        {
            // Check if the requested entity exists.
            if (await _editRepository.GetEditByIdAsync(id) is not Edit edit)
            {
                return NotFound();
            }

            await _editRepository.DeleteEditAsync(edit);

            return NoContent();
        }

        /// <summary>
        /// Handles the exceptions and errors in development environment.
        /// </summary>
        /// <param name="hostEnvironment">Object containing information about the current runtime environment.</param>
        /// <returns>An <see cref="IActionResult"/> which wraps the server response.</returns>
        [Route("/error-dev")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
        {
            // Makes the endpoint unaccessible in non-development environments.
            if (!hostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            // Configure the exception handler to show the error message.
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            return Problem(detail: exceptionHandlerFeature.Error.StackTrace, title: exceptionHandlerFeature.Error.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Handles the exceptions and errors in production environment.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> which wraps the server response.</returns>
        [Route("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError()
        {
            // Configure the exception handler to show the error message.
            return Problem(detail: "An unexpected error occurred. Please try again later.", title: "Internal Server Error", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
