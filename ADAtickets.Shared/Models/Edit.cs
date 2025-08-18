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

using AutoMapper.Configuration.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a modification made to a ticket, either by a user or by the system.
/// </summary>
/// <remarks>This class is intended for internal use only; it is public only to allow for testing.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Edit : Entity
{
    /// <summary>
    ///     The date and time when the edit was made.
    /// </summary>
    [Required]
    public DateTimeOffset EditDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The message the edit comes with.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     The status the ticket was in before the edit.
    /// </summary>
    [Required]
    public Status OldStatus { get; set; } = Status.Unassigned;

    /// <summary>
    ///     The status the ticket will be after the edit.
    /// </summary>
    [Required]
    public Status NewStatus { get; set; } = Status.Unassigned;

    /// <summary>
    ///     The id of the ticket this edit is related to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Ticket))]
    public Guid TicketId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The ticket this edit is related to.
    /// </summary>
    [Required]
    [Ignore]
    [JsonIgnore]
    public Ticket Ticket { get; set; } = null!;

    /// <summary>
    ///     The id of the user who made the edit.
    /// </summary>
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The user who made the edit.
    /// </summary>
    [Required]
    [Ignore]
    [JsonIgnore]
    public User User { get; set; } = null!;
}