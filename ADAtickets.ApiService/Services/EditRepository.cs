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
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Edit"/> entities in the underlying database.
    /// </summary>
    sealed class EditRepository(ADAticketsDbContext context) : IEditRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IEditRepository.GetEditByIdAsync"/>
        public async Task<Edit?> GetEditByIdAsync(Guid id)
        {
            return await _context.Edits.FindAsync(id);
        }

        /// <inheritdoc cref="IEditRepository.GetEditsAsync"/>
        public async Task<IEnumerable<Edit>> GetEditsAsync()
        {
            return await _context.Edits.ToListAsync();
        }

        /// <inheritdoc cref="IEditRepository.GetEditsByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<Edit>> GetEditsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Edit> query = _context.Edits;

            foreach (var filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Edit.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(edit => edit.Id == outGuid);
                        break;

                    case nameof(Edit.TicketId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(edit => edit.TicketId == outGuid);
                        break;

                    case nameof(Edit.UserId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(edit => edit.UserId == outGuid);
                        break;

                    case nameof(Edit.OldStatus) when Enum.TryParse(filter.Value, true, out Status outStatus):
                        query = query.Where(edit => edit.OldStatus == outStatus);
                        break;

                    case nameof(Edit.NewStatus) when Enum.TryParse(filter.Value, true, out Status outStatus):
                        query = query.Where(edit => edit.NewStatus == outStatus);
                        break;

                    case nameof(Edit.EditDateTime) when DateTimeOffset.TryParse(filter.Value, CultureInfo.InvariantCulture, out DateTimeOffset outDateTimeOffset):
                        query = query.Where(edit => edit.EditDateTime.Date == outDateTimeOffset.Date);
                        break;

                    case nameof(Edit.Description):
                        query = query.Where(edit => edit.Description.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IEditRepository.AddEditAsync"/>
        public async Task AddEditAsync(Edit edit)
        {
            _context.Edits.Add(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.UpdateEditAsync"/>
        public async Task UpdateEditAsync(Edit edit)
        {
            _context.Edits.Update(edit);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IEditRepository.DeleteEditAsync"/>
        public async Task DeleteEditAsync(Edit edit)
        {
            _context.Edits.Remove(edit);
            await _context.SaveChangesAsync();
        }
    }
}
