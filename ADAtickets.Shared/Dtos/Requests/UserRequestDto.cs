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
using ADAtickets.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.Shared.Dtos.Requests
{
    /// <summary>
    /// <para>Represents a user of the system.</para>
    /// <para>It is a simplified version of the <see cref="User"/> class, used for data transfer to the server.</para>
    /// </summary>
    public sealed class UserRequestDto : RequestDto
    {
        /// <summary>
        /// The email address of the user.
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The name of the user.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The surname of the user.
        /// </summary>
        [Required]
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// The phone number of the user.
        /// </summary>
        public string? PhoneNumber { get; set; } = null;

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
        public UserType Type { get; set; } = UserType.User;
    }
}
