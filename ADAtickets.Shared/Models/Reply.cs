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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a reply in a ticket comment thread.
/// </summary>
internal sealed class Reply : Entity
{
    /// <summary>
    ///     The date and time when the reply was sent.
    /// </summary>
    [Required]
    public DateTimeOffset ReplyDateTime { get; set; }

    /// <summary>
    ///     The message written in the reply.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    [Unicode]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The id of the user who sent the reply.
    /// </summary>
    [Required]
    [ForeignKey(nameof(AuthorUser))]
    public Guid AuthorUserId { get; set; }

    /// <summary>
    ///     The user who sent the reply.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public User AuthorUser { get; set; } = new();

    /// <summary>
    ///     The id of the ticket this reply was sent to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Ticket))]
    public Guid TicketId { get; set; }

    /// <summary>
    ///     The ticket this reply was sent to.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Ticket Ticket { get; set; } = new();
}