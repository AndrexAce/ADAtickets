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
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents the link between users and sent notifications.
/// </summary>
[Index(nameof(ReceiverUserId), nameof(NotificationId))]
public sealed class UserNotification : Entity
{
    /// <summary>
    ///     The unique identifier of the user who received the notification.
    /// </summary>
    [Required]
    [ForeignKey(nameof(ReceiverAppUser))]
    public Guid ReceiverUserId { get; set; }

    /// <summary>
    ///     The user who received the notification.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public AppUser ReceiverAppUser { get; set; } = new();

    /// <summary>
    ///     The unique identifier of the sent notification.
    /// </summary>
    [Required]
    [ForeignKey(nameof(Notification))]
    public Guid NotificationId { get; set; }

    /// <summary>
    ///     The notification that was sent.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Notification Notification { get; set; } = new();

    /// <summary>
    ///     Whether the user has read the notification.
    /// </summary>
    [Required]
    public bool IsRead { get; set; }
}