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
///     Represents a notification sent to a user triggered by an action like ticket modification, reply, or assignment.
/// </summary>
public sealed class Notification : Entity
{
    /// <summary>
    ///     The date and time when the notification was sent.
    /// </summary>
    [Required]
    public DateTimeOffset SendDateTime { get; set; }

    /// <summary>
    ///     The message the notification comes with.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Unicode]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The id of the ticket this notification is related to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Ticket))]
    public Guid TicketId { get; set; }

    /// <summary>
    ///     The ticket this notification is related to.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    public Ticket Ticket { get; set; } = new();

    /// <summary>
    ///     The id of the user this notification is related to.
    /// </summary>
    [Required]
    [ForeignKey(nameof(AppUser))]
    public Guid UserId { get; set; }

    /// <summary>
    ///     The user this notification is related to.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public AppUser AppUser { get; set; } = new();

    /// <summary>
    ///     Join-entity between the notification and the users it was sent to.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserNotification.Notification))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<UserNotification> UserNotifications { get; } = [];
}