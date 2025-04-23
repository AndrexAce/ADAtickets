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
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents the link between users and their preferred platforms.
    /// </summary>
    [PrimaryKey(nameof(UserEmail), nameof(PlatformName))]
    internal class UserPlatform
    {
        /// <value>
        /// Gets or sets the email of the user who marked the platform as preferred.
        /// </value>
        [MaxLength(254)]
        internal string UserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the user who marked the platform as preferred.
        /// </value>
        internal User User { get; set; } = new User();

        /// <value>
        /// Gets or sets the name of the platform that was marked as preferred.
        /// </value>
        [MaxLength(50)]
        internal string PlatformName { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the platform that was marked as preferred.
        /// </value>
        internal Platform Platform { get; set; } = new Platform();
    }
}
