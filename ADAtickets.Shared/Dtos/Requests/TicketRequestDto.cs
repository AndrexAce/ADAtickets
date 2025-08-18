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

using ADAtickets.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ADAtickets.Shared.Dtos.Requests;

/// <summary>
///     <para>Represents a ticket sent by a user to the system.</para>
///     <para>It is a simplified version of the <see cref="Ticket" /> class, used for data transfer to the server.</para>
/// </summary>
public sealed class TicketRequestDto : RequestDto
{
    /// <summary>
    ///     The type of user request bound to the ticket.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TicketType Type { get; set; } = TicketType.Bug;

    /// <summary>
    ///     The date and time when the ticket was created.
    /// </summary>
    [Required]
    public DateTimeOffset CreationDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The title of the ticket, a brief recap of the issue.
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     The description of the ticket, a detailed description of the issue.
    /// </summary>
    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     The urgency of the ticket.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Priority Priority { get; set; } = Priority.Low;

    /// <summary>
    ///     The status of the ticket.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Status Status { get; set; } = Status.Unassigned;

    /// <summary>
    ///     The id of the work item the ticket is related to in Azure DevOps.
    /// </summary>
    [Required]
    public int WorkItemId { get; set; } = 0;

    /// <summary>
    ///     The id of the platform the ticket is related to.
    /// </summary>
    [Required]
    public Guid PlatformId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The id of the user who created the ticket.
    /// </summary>
    [Required]
    public Guid CreatorUserId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The id of the operator assigned to the ticket.
    /// </summary>
    public Guid? OperatorUserId { get; set; } = null;

    /// <summary>
    ///     Identifier of the <see cref="User" /> making the request.
    /// </summary>
    public Guid? Requester { get; set; } = null;
}