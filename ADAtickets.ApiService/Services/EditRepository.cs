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
using ADAtickets.ApiService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Edit"/> entities in the underlying database.
    /// </summary>
    class EditRepository(ADAticketsDbContext context) : IEditRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IEditRepository.GetEditByIdAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity was not found.</exception>
        public async Task<Edit> GetEditByIdAsync(Guid id)
        {
            return await _context.Edits.FindAsync(id) ?? throw new InvalidOperationException($"Entity of type {typeof(Edit)} with ID {id} was not found.");
        }

        /// <inheritdoc cref="IEditRepository.GetEditsAsync"/>
        public async IAsyncEnumerable<Edit> GetEditsAsync()
        {
            await foreach (var edit in _context.Edits.AsAsyncEnumerable())
            {
                yield return edit;
            }
        }

        /// <inheritdoc cref="IEditRepository.AddEditAsync(Edit)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddEditAsync(Edit edit)
        {

            await _context.Edits.AddAsync(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.UpdateEditAsync(Edit)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflit.</exception>
        public async Task UpdateEditAsync(Edit edit)
        {
            _context.Edits.Update(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.DeleteEditAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>
        public async Task DeleteEditAsync(Guid id)
        {
            if (await _context.Edits.FindAsync(id) is not Edit edit)
                throw new InvalidOperationException($"Entity of type {typeof(Edit)} with ID {id} was not found.");
            _context.Remove(edit);
            await _context.SaveChangesAsync();
        }
    }
}
