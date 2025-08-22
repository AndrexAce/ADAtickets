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
///     Web API controller managing requests involving <see cref="Attachment" /> etities.
/// </summary>
/// <param name="attachmentRepository">Object defining the operations allowed on the entity type.</param>
/// <param name="mapper">
///     Object definining the mappings of fields between the <see cref="Attachment" /> entity and its
///     <see cref="AttachmentRequestDto" /> or <see cref="AttachmentResponseDto" /> correspondant.
/// </param>
/// <param name="ticketRepository">Object defining the operations allowed on the <see cref="Ticket" /> entity type.</param>
/// <param name="platformRepository">Object defining the operations allowed on the <see cref="Platform" /> entity type.</param>
/// <param name="azureDevOpsController">Controller managing the interaction with Azure DevOps.</param>
[Route($"v{Service.APIVersion}/{Controller.Attachments}")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[FormatFilter]
[ApiConventionType(typeof(ApiConventions))]
public sealed class AttachmentsController(
    IAttachmentRepository attachmentRepository,
    IMapper mapper,
    ITicketRepository ticketRepository,
    IPlatformRepository platformRepository,
    AzureDevOpsController azureDevOpsController
) : ControllerBase
{
    /// <summary>
    ///     Fetch all the <see cref="Attachment" /> entities or all the entities respecting the given criteria.
    /// </summary>
    /// <remarks>
    ///     For example, the following request:
    ///     <c>GET /api/Attachments?ticketId=123e4567-e89b-12d3-a456-426614174000&amp;path=example.png</c>
    ///     Retrieves the entities linked to the ticket with id <b>123e4567-e89b-12d3-a456-426614174000</b> and which contain
    ///     <b>example.png</b> in the path.
    /// </remarks>
    /// <param name="filters">
    ///     A group of key-value pairs defining the property name and value <see cref="Attachment" />
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
    public async Task<ActionResult<IEnumerable<AttachmentResponseDto>>> GetAttachments(
        [FromQuery] Dictionary<string, string>? filters)
    {
        var attachments = await (filters != null
            ? attachmentRepository.GetAttachmentsByAsync(filters)
            : attachmentRepository.GetAttachmentsAsync());

        return Ok(attachments.Select(mapper.Map<AttachmentResponseDto>));
    }

    /// <summary>
    ///     Fetch a specific <see cref="Attachment" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="Attachment" /> entity to fetch.</param>
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
    public async Task<ActionResult<AttachmentResponseDto>> GetAttachment(Guid id)
    {
        // Check if the requested entity exists.
        if (await attachmentRepository.GetAttachmentByIdAsync(id) is not Attachment attachment) return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(mapper.Map<AttachmentResponseDto>(attachment));
    }

    /// <summary>
    ///     Update a specific <see cref="Attachment" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "Name": "photo.png",
    ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "Content": [0, 0, 0, 0, 0, 0]
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;AttachmentRequestDto&gt;
    ///     &lt;Name&gt;photo.png&lt;/AttachmentDateTime&gt;
    ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;Content&gt;AAECAwQFBg==&lt;/Content&gt;
    /// &lt;/AttachmentRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="id">Identifier of the <see cref="Attachment" /> entity to update.</param>
    /// <param name="attachmentDto">Object containing the new values the fields should be updated to.</param>
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
    public async Task<ActionResult<AttachmentResponseDto>> PutAttachment(Guid id, AttachmentRequestDto attachmentDto)
    {
        // If the requested entity does not exist, create a new one.
        if (await attachmentRepository.GetAttachmentByIdAsync(id) is not Attachment attachment)
            return await PostAttachment(attachmentDto);

        try
        {
            // Update the existing entity with the new data.
            await attachmentRepository.UpdateAttachmentAsync(mapper.Map(attachmentDto, attachment),
                attachmentDto.Content, attachment.Path);
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the entity is not found in the data source, it was deleted by another user while updating.
            if (await attachmentRepository.GetAttachmentByIdAsync(id) is null) return NotFound();

            // If the entity is found, it was modified by another at the same time of the update.
            return Conflict();
        }

        return NoContent();
    }

    /// <summary>
    ///     Create a new <see cref="Attachment" /> entity.
    /// </summary>
    /// <remarks>
    ///     JSON request body example:
    ///     <code>
    /// {
    ///     "Name": "photo.png",
    ///     "TicketId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "Content": [0, 0, 0, 0, 0, 0]
    /// }
    /// </code>
    ///     XML request body example:
    ///     <code>
    /// &lt;AttachmentRequestDto&gt;
    ///     &lt;Name&gt;photo.png&lt;/AttachmentDateTime&gt;
    ///     &lt;TicketId&gt;123e4567-e89b-12d3-a456-426614174000&lt;/TicketId&gt;
    ///     &lt;Content&gt;AAECAwQFBg==&lt;/Content&gt;
    /// &lt;/AttachmentRequestDto&gt;
    /// </code>
    /// </remarks>
    /// <param name="attachmentDto">Object containing the values the new entity should have.</param>
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
    [Authorize(Policy = Policy.UserOnly)]
    [RequiredScope(Scope.Read, Scope.Write)]
    public async Task<ActionResult<AttachmentResponseDto>> PostAttachment(AttachmentRequestDto attachmentDto)
    {
        var attachment = mapper.Map<Attachment>(attachmentDto);

        // Insert the DTO info into a new entity and add it to the data source.
        await attachmentRepository.AddAttachmentAsync(attachment, attachmentDto.Content);

        await ProcessAttachmentCreationAsync(attachment);

        // Return the created entity and its location to the client.
        return CreatedAtAction(nameof(GetAttachment), new { id = attachment.Id },
            mapper.Map<AttachmentResponseDto>(attachment));
    }

    /// <summary>
    ///     Delete a specific <see cref="Attachment" /> entity.
    /// </summary>
    /// <param name="id">Identifier of the <see cref="Attachment" /> entity to delete.</param>
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
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        // Check if the requested entity exists.
        if (await attachmentRepository.GetAttachmentByIdAsync(id) is not Attachment attachment) return NotFound();

        await attachmentRepository.DeleteAttachmentAsync(attachment);

        return NoContent();
    }

    private async Task ProcessAttachmentCreationAsync(Attachment attachment)
    {
        if (await ticketRepository.GetTicketByIdAsync(attachment.TicketId) is Ticket ticket &&
           await platformRepository.GetPlatformByIdAsync(ticket.PlatformId) is Platform platform)
        {
            await azureDevOpsController.CreateAttachmentAzureDevOpsWorkItemAsync(ticket.WorkItemId, attachment.Path, platform.Name);
        }
    }
}