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
///     Exposes the methods to manage the <see cref="Attachment" /> entities from a data source.
/// </summary>
public interface IAttachmentRepository
{
    /// <summary>
    ///     Gets a single <see cref="Attachment" /> entity from the data source asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="Attachment" /> entity.</param>
    /// <returns>
    ///     A <see cref="Task" /> returning the <see cref="Attachment" /> with the given <paramref name="id" />, or
    ///     <see langword="null" /> if it doesn't exist.
    /// </returns>
    Task<Attachment?> GetAttachmentByIdAsync(Guid id);

    /// <summary>
    ///     Gets all <see cref="Attachment" /> entities from the data source asynchronously.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task" /> returning all the <see cref="Attachment" /> entities, or an empty collection if there
    ///     are none.
    /// </returns>
    Task<IEnumerable<Attachment>> GetAttachmentsAsync();

    /// <summary>
    ///     Gets all <see cref="Attachment" /> entities from the data source which meet the given criteria asynchronously.
    /// </summary>
    /// <param name="filters">
    ///     Group of key-value pairs representing the criteria to filter the <see cref="Attachment" />
    ///     entities.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> returning all the <see cref="Attachment" /> entities, or an empty collection if there
    ///     are none.
    /// </returns>
    Task<IEnumerable<Attachment>> GetAttachmentsByAsync(IEnumerable<KeyValuePair<string, string>> filters);

    /// <summary>
    ///     Adds a new <see cref="Attachment" /> entity to the data source asynchronously.
    /// </summary>
    /// <param name="attachment">The <see cref="Attachment" /> entity to add to the data source.</param>
    /// <param name="data">The content of the attachment to save on the server.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task AddAttachmentAsync(Attachment attachment, byte[] data);

    /// <summary>
    ///     Updates an existing <see cref="Attachment" /> entity in the data source asynchronously.
    /// </summary>
    /// <param name="attachment">The <see cref="Attachment" /> entity to update in the data source.</param>
    /// <param name="data">The content of the attachment to update on the server.</param>
    /// <param name="oldAttachmentPath">The path of the old attachment to be updated.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task UpdateAttachmentAsync(Attachment attachment, byte[] data, string oldAttachmentPath);

    /// <summary>
    ///     Deletes an <see cref="Attachment" /> entity from the data source asynchronously.
    /// </summary>
    /// <param name="attachment">The <see cref="Attachment" /> entity to delete in the data source.</param>
    /// <returns>A <see cref="Task" /> executing the action.</returns>
    Task DeleteAttachmentAsync(Attachment attachment);
}