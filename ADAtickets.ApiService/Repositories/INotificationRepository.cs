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
    /// Exposes the methods to manage the <see cref="Notification"/> entities from a data source.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Gets a single <see cref="Notification"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Notification"/> entity.</param>
        /// <returns>A <see cref="Task"/> returning the <see cref="Notification"/> with the given <paramref name="id"/>.</returns>
        Task<Notification> GetNotificationByIdAsync(Guid id);
        /// <summary>
        /// Gets all <see cref="Notification"/> entities from the data source asynchronously.
        /// </summary>
        /// <returns>An asynchronously enumerable object containing all the <see cref="Notification"/> entities.</returns>
        IAsyncEnumerable<Notification> GetNotificationsAsync();
        /// <summary>
        /// Adds a new <see cref="Notification"/> entity to the data source asynchronously.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> entity to add to the data source.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task AddNotificationAsync(Notification notification);
        /// <summary>
        /// Updates an existing <see cref="Notification"/> entity in the data source asynchronously.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> entity to update in the data source.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task UpdateNotificationAsync(Notification notification);
        /// <summary>
        /// Deletes an <see cref="Notification"/> entity from the data source asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the <see cref="Notification"/> entity.</param>
        /// <returns>A <see cref="Task"/> executing the action.</returns>
        Task DeleteNotificationAsync(Guid id);
    }
}