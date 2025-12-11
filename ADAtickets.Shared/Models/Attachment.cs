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
///     Represents an attachment associated with a ticket.
/// </summary>
[Index(nameof(ObjectKey), IsUnique = true)]
internal sealed class Attachment : Entity
{
    /// <summary>
    ///     The MinIO object key of the attachment.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Unicode(false)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    ///     The id of the ticket this attachment is related to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Ticket))]
    public Guid TicketId { get; set; }

    /// <summary>
    ///     The ticket this attachment is related to.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Ticket Ticket { get; set; } = new();
}