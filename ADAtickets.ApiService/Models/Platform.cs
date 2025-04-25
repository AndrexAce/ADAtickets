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
    /// Represents a platform managed by the enterprise which tickets are related to.
    /// </summary>
    [PrimaryKey(nameof(Name), nameof(RepositoryUrl))]
    sealed class Platform : EntityBase
    {
        /// <value>
        /// Gets or sets the name of the platform.
        /// </value>
        [MaxLength(254)]
        public string Name { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the URL of the repository where the source code of the platform is hosted.
        /// </value>
        [MaxLength(4000)]
        public string RepositoryUrl { get; set; } = string.Empty;

        /// <value>
        /// Gets the collection of the tickets related to the platform.
        /// </value>
        public ICollection<Ticket> Tickets { get; } = [];

        /// <value>
        /// Gets the collection of users who marked the platform as preferred.
        /// </value>
        public ICollection<UserPlatform> PreferredPlatforms { get; } = [];
    }
}
