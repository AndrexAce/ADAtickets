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

namespace ADAtickets.ApiService.Repositories;

/// <summary>
///     Exposes the methods to manage the <see cref="UserPlatform" /> entities from a data source.
/// </summary>
public interface IUserPlatformRepository
{
    /// <summary>
    ///     Gets a single <see cref="UserPlatform" /> entity from the data source asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="UserPlatform" /> entity.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning the <see cref="UserPlatform" /> with the given <paramref name="id" />, or
    ///     <see langword="null" /> if it doesn't exist..
    /// </returns>
    Task<UserPlatform?> GetUserPlatformByIdAsync(Guid id);

    /// <summary>
    ///     Gets all <see cref="UserPlatform" /> entities from the data source asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task" /> returning all the <see cref="UserPlatform" /> entities, or an empty collection if there
    ///     are none.
    /// </returns>
    Task<IEnumerable<UserPlatform>> GetUserPlatformsAsync();

    /// <summary>
    ///     Gets all <see cref="UserPlatform" /> entities from the data source which meet the given criteria asynchronously.
    /// </summary>
    /// <param name="filters">
    ///     Group of key-value pairs representing the criteria to filter the <see cref="UserPlatform" />
    ///     entities.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> returning all the <see cref="UserPlatform" /> entities, or an empty collection if there
    ///     are none.
    /// </returns>
    Task<IEnumerable<UserPlatform>> GetUserPlatformsByAsync(IEnumerable<KeyValuePair<string, string>> filters);

    /// <summary>
    ///     Adds a new <see cref="UserPlatform" /> entity to the data source asynchronously.
    /// </summary>
    /// <param name="UserPlatform">The <see cref="UserPlatform" /> entity to add to the data source.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task AddUserPlatformAsync(UserPlatform UserPlatform);

    /// <summary>
    ///     Updates an existing <see cref="UserPlatform" /> entity in the data source asynchronously.
    /// </summary>
    /// <param name="UserPlatform">The <see cref="UserPlatform" /> entity to update in the data source.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task UpdateUserPlatformAsync(UserPlatform UserPlatform);

    /// <summary>
    ///     Deletes an <see cref="UserPlatform" /> entity from the data source asynchronously.
    /// </summary>
    /// <param name="UserPlatform">The <see cref="UserPlatform" /> entity to delete in the data source.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task DeleteUserPlatformAsync(UserPlatform UserPlatform);
}