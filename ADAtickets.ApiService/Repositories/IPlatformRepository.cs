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

namespace ADAtickets.ApiService.Repositories
{
    /// <summary>
    /// Exposes the methods to manage the <see cref="Platform"/> entities from a data source.
    /// </summary>
    internal interface IPlatformRepository
    {
        /// <summary>
        /// Gets a single <see cref="Platform"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="name">The unique identifier of the <see cref="Platform"/> entity.</param>
        /// <returns>A <see cref="Task"/> returning the <see cref="Platform"/> with the given <paramref name="name"/>.</returns>
        Task<Platform> GetPlatformByNameAsync(string name);
        /// <summary>
        /// Gets all <see cref="Platform"/> entities from the data source asynchronously.
        /// </summary>
        /// <returns>An asynchronously enumerable object containing all the <see cref="Platform"/> entities.</returns>
        IAsyncEnumerable<Platform> GetPlatformsAsync();
        /// <summary>
        /// Adds a new <see cref="Platform"/> entity to the data source asynchronously.
        /// </summary>
        /// <param name="Platform">The <see cref="Platform"/> entity to add to the data source.</param>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if the entity was added successfully, and <see langword="false"/> otherwise.</returns>
        Task<bool> AddPlatformAsync(Platform platform);
        /// <summary>
        /// Updates an existing <see cref="Platform"/> entity in the data source asynchronously.
        /// </summary>
        /// <param name="Platform">The <see cref="Platform"/> entity to update in the data source.</param>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if the entity was updated successfully, and <see langword="false"/> otherwise.</returns>
        Task<bool> UpdatePlatformAsync(Platform platform);
        /// <summary>
        /// Deletes an <see cref="Platform"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="name">The unique identifier of the <see cref="Platform"/> entity.</param>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if the entity was deleted successfully, and <see langword="false"/> otherwise.</returns>
        Task<bool> DeletePlatformAsync(string name);
    }
}