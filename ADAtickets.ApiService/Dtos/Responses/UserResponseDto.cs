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
using ADAtickets.ApiService.Models;

namespace ADAtickets.ApiService.Dtos.Responses
{
    /// <summary>
    /// <para>Represents a user of the system.</para>
    /// <para>It is a simplified version of the <see cref="User"/> class, used for data transfer to the client.</para>
    /// </summary>
    public sealed class UserResponseDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The surname of the user.
        /// </summary>
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user enabled the two-factor authentication via email.
        /// </summary>
        public bool IsEmail2FAEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the two-factor authentication via SMS.
        /// </summary>
        public bool IsPhone2FAEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the reception of external notifications via email.
        /// </summary>
        public bool AreEmailNotificationsEnabled { get; set; } = false;

        /// <summary>
        /// Whether the user enabled the reception of external notifications via SMS.
        /// </summary>
        public bool ArePhoneNotificationsEnabled { get; set; } = false;

        /// <summary>
        /// The role of the user in the system.
        /// </summary>
        public UserType Type { get; set; } = UserType.User;

        /// <summary>
        /// The unique identifier of the user's Microsoft account.
        /// </summary>
        public string? MicrosoftAccountId { get; set; } = null;

        /// <summary>
        /// The unique identifier of the user's ASP.NET Identity User.
        /// </summary>
        public Guid IdentityUserId { get; set; } = Guid.Empty;

        /// <summary>
        /// The collection of ids of tickets created by the user (if they are a user, otherwise it must be empty).
        /// </summary>
        public ICollection<Guid> CreatedTickets { get; } = [];

        /// <summary>
        /// The collection of ids of tickets assigned to the user (if they are an operator, otherwise it must be empty).
        /// </summary>
        public ICollection<Guid> AssignedTickets { get; } = [];

        /// <summary>
        /// The collection of ids of replies sent by the user to any ticket.
        /// </summary>
        public ICollection<Guid> Replies { get; } = [];

        /// <summary>
        /// The collection of ids of edits made by the user to any ticket.
        /// </summary>
        public ICollection<Guid> Edits { get; } = [];

        /// <summary>
        /// The collection of ids of platforms the user prefers (if they are an operator, otherwise it must be empty).
        /// </summary>
        public ICollection<Guid> PreferredPlatforms { get; } = [];

        /// <summary>
        /// The collection of ids of notifications the user triggered the sending of.
        /// </summary>
        public ICollection<Guid> SentNotifications { get; } = [];

        /// <summary>
        /// The collection of ids of notifications the user received.
        /// </summary>
        public ICollection<Guid> ReceivedNotifications { get; } = [];
    }
}
