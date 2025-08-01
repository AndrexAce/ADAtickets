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
using ADAtickets.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.Shared.Dtos.Requests
{
    /// <summary>
    /// <para>Represents the link between a user and their preferred platform.</para>
    /// <para>It is a simplified version of the <see cref="UserPlatform"/> class, used for data transfer to the server.</para>
    /// </summary>
    public sealed class UserPlatformRequestDto : RequestDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        [Required]
        public Guid UserId { get; set; } = Guid.Empty;

        /// <summary>
        /// The unique identifier of the platform.
        /// </summary>
        [Required]
        public Guid PlatformId { get; set; } = Guid.Empty;
    }
}
