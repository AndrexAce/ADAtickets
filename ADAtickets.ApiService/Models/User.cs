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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents a user of the system.
    /// </summary>
    sealed class User : EntityBase
    {
        /// <value>
        /// Gets or sets the email of the user.
        /// </value>
        [Key]
        [MaxLength(254)]
        internal string Email { get; set; } = string.Empty;
        /// <value>
        /// <para>Gets or sets the password of the user.</para>
        /// <para>The password should is hashed so that it is not internally visible.</para>
        /// </value>
        [MaxLength(64)]
        internal string Password { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the name of the user.
        /// </value>
        [MaxLength(50)]
        internal string Name { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the surname of the user.
        /// </value>
        [MaxLength(50)]
        internal string Surname { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the phone number of the user.
        /// </value>
        [MaxLength(20)]
        internal string PhoneNumber { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets whether the user enabled the two factor authentication via email.
        /// </value>
        internal bool IsEmail2FAEnabled { get; set; } = false;
        /// <value>
        /// Gets or sets whether the user enabled the two factor authentication via SMS.
        /// </value>
        internal bool IsPhone2FAEnabled { get; set; } = false;
        /// <value>
        /// Gets or sets whether the user enabled the reception of external notifications via email.
        /// </value>
        internal bool AreEmailNotificationsEnabled { get; set; } = false;
        /// <value>
        /// Gets or sets whether the user enabled the reception of external notifications via SMS.
        /// </value>
        internal bool ArePhoneNotificationsEnabled { get; set; } = false;
        /// <value>
        /// Gets or sets the role of the user in the system.
        /// </value>
        internal UserType Type { get; set; } = UserType.USER;
        /// <value>
        /// Gets or sets the unique identifier of the user's Microsoft account.
        /// </value>
        [MaxLength(20)]
        internal string MicrosoftAccountId { get; set; } = string.Empty;

        /// <value>
        /// Gets the collection of tickets created by the user (if he is a user, otherwise it must be empty).
        /// </value>
        [InverseProperty("CreatorUser")]
        internal ICollection<Ticket> CreatedTickets { get; } = [];

        /// <value>
        /// Gets the collection of tickets assigned to the user (if he is an operator, otherwise it must be empty).
        /// </value>
        [InverseProperty("OperatorUser")]
        internal ICollection<Ticket> AssignedTickets { get; } = [];

        /// <value>
        /// Gets the collection of replies sent by the user to any ticket.
        /// </value>
        internal ICollection<Reply> Replies { get; } = [];

        /// <value>
        /// Gets the collection of edits made by the user to any ticket.
        /// </value>
        internal ICollection<Edit> Edits { get; } = [];

        /// <value>
        /// Gets the collection of platforms the user prefers (if he is an operator, otherwise it must be empty).
        /// </value>
        internal ICollection<UserPlatform> PreferredPlatforms { get; } = [];

        /// <value>
        /// Gets the collection of notifications the user triggered the sending of.
        /// </value>
        internal ICollection<Notification> SentNotifications { get; } = [];

        /// <value>
        /// Gets the collection of notifications the user received.
        /// </value>
        internal ICollection<UserNotification> ReceivedNotifications { get; } = [];
    }
}
