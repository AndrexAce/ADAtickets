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
using System.Text.Json.Serialization;
using ADAtickets.Shared.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a ticket sent by a user to the system.
/// </summary>
public sealed class Ticket : Entity
{
    /// <summary>
    ///     The type of user request bound to the ticket.
    /// </summary>
    [Required]
    public TicketType Type { get; set; }

    /// <summary>
    ///     The date and time when the ticket was created.
    /// </summary>
    [Required]
    public DateTimeOffset CreationDateTime { get; set; }

    /// <summary>
    ///     The title of the ticket, a brief recap of the issue.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Unicode]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     The description of the ticket, a detailed description of the issue.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    [Unicode]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     The urgency of the ticket.
    /// </summary>
    [Required]
    public Priority Priority { get; set; }

    /// <summary>
    ///     The status of the ticket.
    /// </summary>
    [Required]
    public Status Status { get; set; }

    /// <summary>
    ///     The id of the work item the ticket is related to in Azure DevOps.
    /// </summary>
    [Required]
    public uint WorkItemId { get; set; }

    /// <summary>
    ///     The id of the platform the ticket is related to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Platform))]
    public Guid PlatformId { get; set; }

    /// <summary>
    ///     The platform the ticket is related to.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Platform Platform { get; set; } = new();

    /// <summary>
    ///     The id of the user who created the ticket.
    /// </summary>
    [Required]
    [ForeignKey(nameof(CreatorAppUser))]
    public Guid CreatorUserId { get; set; }

    /// <summary>
    ///     The user who created the ticket.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public AppUser CreatorAppUser { get; set; } = new();

    /// <summary>
    ///     The id of the operator assigned to the ticket.
    /// </summary>
    [ForeignKey(nameof(OperatorUser))]
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    ///     The operator assigned to the ticket.
    /// </summary>
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public AppUser? OperatorUser { get; set; }

    /// <summary>
    ///     The collection of edits made to the ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Edit.Ticket))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Edit> Edits { get; } = [];

    /// <summary>
    ///     The collection of replies sent to the ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Reply.Ticket))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Reply> Replies { get; } = [];

    /// <summary>
    ///     The collection of attachments attached to the ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Attachment.Ticket))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Attachment> Attachments { get; } = [];

    /// <summary>
    ///     The collection of notifications related to the ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Notification.Ticket))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Notification> Notifications { get; } = [];
}