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
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Platform"/> entities in the underlying database.
    /// </summary>
    class PlatformRepository(ADAticketsDbContext context) : IPlatformRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IPlatformRepository.GetPlatformByIdAsync"/>
        public async Task<Platform?> GetPlatformByIdAsync(Guid id)
        {
            return await _context.Platforms.FindAsync(id);
        }

        /// <inheritdoc cref="IPlatformRepository.GetPlatformsAsync"/>
        public async Task<IEnumerable<Platform>> GetPlatformsAsync()
        {
            return await _context.Platforms.ToListAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.GetPlatformsByAsync"/>
        public async Task<IEnumerable<Platform>> GetPlatformsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Platform> query = _context.Platforms;

            foreach (var filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Platform.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(platform => platform.Id == outGuid);
                        break;

                    case nameof(Platform.Name):
                        query = query.Where(platform => platform.Name.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case nameof(Platform.RepositoryUrl):
                        query = query.Where(platform => platform.RepositoryUrl.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.AddPlatformAsync"/>
        public async Task AddPlatformAsync(Platform platform)
        {
            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.UpdatePlatformAsync"/>
        public async Task UpdatePlatformAsync(Platform platform)
        {
            _context.Platforms.Update(platform);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.DeletePlatformAsync"/>
        public async Task DeletePlatformAsync(Platform platform)
        {
            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
        }
    }
}
