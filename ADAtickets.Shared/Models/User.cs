﻿/*
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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a user of the system.
/// </summary>
/// <remarks>This class is intended for internal use only; it is public only to allow for testing.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[Index(nameof(Email), IsUnique = true)]
public sealed class User : Entity
{
    /// <summary>
    ///     The email address of the user.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     The username of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     The name of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The surname of the user.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the user enabled the reception of external notifications via email.
    /// </summary>
    [Required]
    public bool AreEmailNotificationsEnabled { get; set; } = false;

    /// <summary>
    ///     Whether the user can login the application.
    /// </summary>
    [Required]
    public bool IsBlocked { get; init; } = false;

    /// <summary>
    ///     The role of the user in the system.
    /// </summary>
    [Required]
    public UserType Type { get; set; } = UserType.User;

    /// <summary>
    ///     The collection of tickets created by the user (if they are a user, otherwise it must be empty).
    /// </summary>
    [Required]
    [InverseProperty(nameof(Ticket.CreatorUser))]
    [JsonIgnore]
    public ICollection<Ticket> CreatedTickets { get; } = [];

    /// <summary>
    ///     The collection of tickets assigned to the user (if they are an operator, otherwise it must be empty).
    /// </summary>
    [Required]
    [InverseProperty(nameof(Ticket.OperatorUser))]
    [JsonIgnore]
    public ICollection<Ticket> AssignedTickets { get; } = [];

    /// <summary>
    ///     The collection of replies sent by the user to any ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Reply.AuthorUser))]
    [JsonIgnore]
    public ICollection<Reply> Replies { get; } = [];

    /// <summary>
    ///     The collection of edits made by the user to any ticket.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Edit.User))]
    [JsonIgnore]
    public ICollection<Edit> Edits { get; } = [];

    /// <summary>
    ///     Join entity between the platform and the users who marked it as preferred.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserPlatform.User))]
    [Ignore]
    [JsonIgnore]
    public ICollection<UserPlatform> UserPlatforms { get; } = [];

    /// <summary>
    ///     The collection of notifications the user triggered the sending of.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Notification.User))]
    [JsonIgnore]
    public ICollection<Notification> SentNotifications { get; } = [];

    /// <summary>
    ///     Join entity between the user and the notifications they received.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserNotification.ReceiverUser))]
    [Ignore]
    [JsonIgnore]
    public ICollection<UserNotification> UserNotifications { get; } = [];
}