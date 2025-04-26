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
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Edit"/> entities in the underlying database.
    /// </summary>
    class EditRepository(ADAticketsDbContext context) : IEditRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IEditRepository.GetEditByIdAsync(Guid)"/>
        public async Task<Edit?> GetEditByIdAsync(Guid id)
        {
            return await _context.Edits.FindAsync(id);
        }

        /// <inheritdoc cref="IEditRepository.GetEdits"/>
        public IAsyncEnumerable<Edit> GetEdits()
        {
            return _context.Edits.AsAsyncEnumerable();
        }

        /// <inheritdoc cref="IEditRepository.AddEditAsync(Edit)"/>
        public async Task AddEditAsync(Edit edit)
        {
            _context.Edits.Add(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.UpdateEditAsync(Edit)"/>
        public async Task UpdateEditAsync(Edit edit)
        {
            _context.Edits.Update(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.DeleteEditAsync(Edit)"/>
        public async Task DeleteEditAsync(Edit edit)
        {
            _context.Remove(edit);
            await _context.SaveChangesAsync();
        }
    }
}
