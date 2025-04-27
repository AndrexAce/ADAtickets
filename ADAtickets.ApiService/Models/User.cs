/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents a user of the system.
    /// </summary>
    public sealed class User : EntityBase
    {
        /// <summary>
        /// The email of the user.
        /// </summary>
        [Key]
        [Required]
        [MaxLength(254)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// <para>The password of the user.</para>
        /// <para>The password is hashed so that it is not internally visible.</para>
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The name of the user.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The surname of the user.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// The phone number of the user.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; } = null;

        /// <summary>
        /// Whether the user enabled the two-factor authentication via email.
        /// </summary>
        [Required]
        public bool IsEmail2FAEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the two-factor authentication via SMS.
        /// </summary>
        [Required]
        public bool IsPhone2FAEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the reception of external notifications via email.
        /// </summary>
        [Required]
        public bool AreEmailNotificationsEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the reception of external notifications via SMS.
        /// </summary>
        [Required]
        public bool ArePhoneNotificationsEnabled { get; set; } = false;

        /// <summary>
        /// The role of the user in the system.
        /// </summary>
        [Required]
        public UserType Type { get; set; } = UserType.USER;

        /// <summary>
        /// The unique identifier of the user's Microsoft account.
        /// </summary>
        [MaxLength(20)]
        public string? MicrosoftAccountId { get; set; } = null;

        /// <summary>
        /// The collection of tickets created by the user (if they are a user, otherwise it must be empty).
        /// </summary>
        [Required]
        [InverseProperty(nameof(Ticket.CreatorUser))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Ticket> CreatedTickets { get; } = [];

        /// <summary>
        /// The collection of tickets assigned to the user (if they are an operator, otherwise it must be empty).
        /// </summary>
        [Required]
        [InverseProperty(nameof(Ticket.OperatorUser))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Ticket> AssignedTickets { get; } = [];

        /// <summary>
        /// The collection of replies sent by the user to any ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Reply.AuthorUser))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Reply> Replies { get; } = [];

        /// <summary>
        /// The collection of edits made by the user to any ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Edit.User))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Edit> Edits { get; } = [];

        /// <summary>
        /// The collection of platforms the user prefers (if they are an operator, otherwise it must be empty).
        /// </summary>
        [Required]
        [InverseProperty(nameof(UserPlatform.User))]
        [Ignore]
        [JsonIgnore]
        public ICollection<UserPlatform> PreferredPlatforms { get; } = [];

        /// <summary>
        /// The collection of notifications the user triggered the sending of.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Notification.User))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Notification> SentNotifications { get; } = [];

        /// <summary>
        /// The collection of notifications the user received.
        /// </summary>
        [Required]
        [InverseProperty(nameof(UserNotification.ReceiverUser))]
        [Ignore]
        [JsonIgnore]
        public ICollection<UserNotification> ReceivedNotifications { get; } = [];
    }
}
