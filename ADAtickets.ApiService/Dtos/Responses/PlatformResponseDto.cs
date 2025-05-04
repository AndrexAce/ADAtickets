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
    /// <para>Represents a platform managed by the enterprise which tickets are related to.</para>
    /// <para>It is a simplified version of the <see cref="Platform"/> class, used for data transfer to the client.</para>
    /// </summary>
    public sealed class PlatformResponseDto
    {
        /// <summary>
        /// The unique identifier of the platform.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the platform.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the repository where the source code of the platform is hosted.
        /// </summary>
        public string RepositoryUrl { get; set; } = string.Empty;

        /// <summary>
        /// The collection of ids of the tickets related to the platform.
        /// </summary>
        public ICollection<Guid> Tickets { get; } = [];

        /// <summary>
        /// The collection of ids of the users who preferred the platform.
        /// </summary>
        public ICollection<Guid> UsersPreferred { get; } = [];
    }
}
