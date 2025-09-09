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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADAtickets.Shared.Dtos.Responses;

/// <summary>
///     <para>Represents a modification made to a ticket, either by a user or by the system.</para>
///     <para>It is a simplified version of the <see cref="Edit" /> class, used for data transfer to the client.</para>
/// </summary>
public sealed record EditResponseDto : ResponseDto
{
    /// <summary>
    ///     The date and time when the edit was made.
    /// </summary>
    public DateTimeOffset EditDateTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The message the edit comes with.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    ///     The status the ticket was in before the edit.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public Status OldStatus { get; init; } = Status.Unassigned;

    /// <summary>
    ///     The status the ticket will be after the edit.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public Status NewStatus { get; init; } = Status.Unassigned;

    /// <summary>
    ///     The id of the ticket this edit is related to.
    /// </summary>
    public Guid TicketId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The id of the user who made the edit.
    /// </summary>
    public Guid UserId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The name of the user responsible for the edit (from related entities <see cref="Notification" /> and <see cref="User" />).
    /// </summary>
    [ValueFromRelationship]
    public string SenderName { get; init; } = string.Empty;
}