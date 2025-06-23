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

namespace ADAtickets.ApiService.Repositories
{
    /// <summary>
    /// Exposes the methods to manage the <see cref="Edit"/> entities from a data source.
    /// </summary>
    public interface IEditRepository
    {
        /// <summary>
        /// Gets a single <see cref="Edit"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Edit"/> entity.</param>
        /// <returns>A <see cref="Task"/> returning the <see cref="Edit"/> with the given <paramref name="id"/>, or <see langword="null"/> if it doesn't exist.</returns>
        Task<Edit?> GetEditByIdAsync(Guid id);

        /// <summary>
        /// Gets all <see cref="Edit"/> entities from the data source asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> returning all the <see cref="Edit"/> entities, or an empty collection if there are none.</returns>
        Task<IEnumerable<Edit>> GetEditsAsync();

        /// <summary>
        /// Gets all <see cref="Edit"/> entities from the data source which meet the given criteria asynchronously.
        /// </summary>
        /// <param name="filters">Group of key-value pairs representing the criteria to filter the <see cref="Edit"/> entities.</param>
        /// <returns>A <see cref="Task"/> returning all the <see cref="Edit"/> entities, or an empty collection if there are none.</returns>
        Task<IEnumerable<Edit>> GetEditsByAsync(IEnumerable<KeyValuePair<string, string>> filters);

        /// <summary>
        /// Adds a new <see cref="Edit"/> entity to the data source asynchronously.
        /// </summary>
        /// <param name="edit">The <see cref="Edit"/> entity to add to the data source.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task AddEditAsync(Edit edit);

        /// <summary>
        /// Updates an existing <see cref="Edit"/> entity in the data source asynchronously.
        /// </summary>
        /// <param name="edit">The <see cref="Edit"/> entity to update in the data source.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task UpdateEditAsync(Edit edit);

        /// <summary>
        /// Deletes an <see cref="Edit"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="edit">The <see cref="Edit"/> entity to delete in the data source.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task DeleteEditAsync(Edit edit);
    }
}