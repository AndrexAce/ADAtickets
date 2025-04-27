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
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents the link between users and their preferred platforms.
    /// </summary>
    [PrimaryKey(nameof(UserEmail), nameof(PlatformName))]
    public class UserPlatform
    {
        /// <summary>
        /// The email of the user who marked the platform as preferred.
        /// </summary>
        [Required]
        [ForeignKey(nameof(User))]
        [MaxLength(254)]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// The user who marked the platform as preferred.
        /// </summary>
        [Required]
        [Ignore]
        [JsonIgnore]
        public User User { get; set; } = new User();

        /// <summary>
        /// The name of the platform that was marked as preferred.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// The platform that was marked as preferred.
        /// </summary>
        [Required]
        [Ignore]
        [JsonIgnore]
        public Platform Platform { get; set; } = new Platform();
    }
}
