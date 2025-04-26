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
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Platform"/> entities in the underlying database.
    /// </summary>
    class PlatformRepository(ADAticketsDbContext context) : IPlatformRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IPlatformRepository.GetPlatformByNameAsync(string)"/>
        /// <exception cref="InvalidOperationException">When the entity was not found.</exception>
        public async Task<Platform> GetPlatformByNameAsync(string name)
        {
            return await _context.Platforms.FindAsync(name) ?? throw new InvalidOperationException($"Entity of type {typeof(Platform)} with name {name} was not found.");
        }

        /// <inheritdoc cref="IPlatformRepository.GetPlatformsAsync"/>
        public async IAsyncEnumerable<Platform> GetPlatformsAsync()
        {
            await foreach (var platform in _context.Platforms.AsAsyncEnumerable())
            {
                yield return platform;
            }
        }

        /// <inheritdoc cref="IPlatformRepository.AddPlatformAsync(Platform)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddPlatformAsync(Platform platform)
        {
            await _context.Platforms.AddAsync(platform);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.UpdatePlatformAsync(Platform)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflict.</exception>
        public async Task UpdatePlatformAsync(Platform platform)
        {
            _context.Platforms.Update(platform);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IPlatformRepository.DeletePlatformAsync(string)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>""
        public async Task DeletePlatformAsync(string name)
        {
            if (await _context.Platforms.FindAsync(name) is not Platform platform)
                throw new InvalidOperationException($"Entity of type {typeof(Platform)} with name {name} was not found.");
            _context.Remove(platform);
            await _context.SaveChangesAsync();
        }
    }
}
