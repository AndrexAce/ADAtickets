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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a user of the system.
/// </summary>
[Index(nameof(IdentityUserId), IsUnique = true)]
public sealed class AppUser : Entity
{
    /// <summary>
    ///     The id of the identity associated with the user.
    /// </summary>
    [Required]
    [ForeignKey(nameof(IdentityUser))]
    public Guid IdentityUserId { get; set; }

    /// <summary>
    ///     The identity associated with the user.
    /// </summary>
    [Required]
    [AdaptIgnore]
    [JsonIgnore]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public IdentityUser<Guid> IdentityUser { get; set; } = new();

    /// <summary>
    ///     The name of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Unicode]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The surname of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Unicode]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    ///     The collection of tickets created by the user (if they are a user, otherwise it must be empty).
    /// </summary>
    [Required]
    [InverseProperty(nameof(Ticket.CreatorAppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Ticket> CreatedTickets { get; } = [];

    /// <summary>
    ///     The collection of tickets assigned to the user (if they are an operator, otherwise it must be empty).
    /// </summary>
    [Required]
    [InverseProperty(nameof(Ticket.OperatorUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Ticket> AssignedTickets { get; } = [];

    /// <summary>
    ///     The collection of replies sent by the user to any ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Reply.AuthorAppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Reply> Replies { get; } = [];

    /// <summary>
    ///     The collection of edits made by the user to any ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Edit.AppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Edit> Edits { get; } = [];

    /// <summary>
    ///     Join-entity between the platform and the users who marked it as preferred.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserPlatform.AppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<UserPlatform> UserPlatforms { get; } = [];

    /// <summary>
    ///     The collection of notifications the user triggered the sending of.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Notification.AppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Notification> SentNotifications { get; } = [];

    /// <summary>
    ///     Join-entity between the user and the notifications they received.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserNotification.ReceiverAppUser))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<UserNotification> UserNotifications { get; } = [];
}