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
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="UserPlatform"/> entities in the underlying database.
    /// </summary>
    internal class UserPlatformRepository(ADAticketsDbContext context) : IUserPlatformRepository
    {
        /// <inheritdoc cref="IUserPlatformRepository.GetUserPlatformByIdAsync"/>
        public async Task<UserPlatform?> GetUserPlatformByIdAsync(Guid id)
        {
            return await context.UserPlatforms.FindAsync(id);
        }

        /// <inheritdoc cref="IUserPlatformRepository.GetUserPlatformsAsync"/>
        public async Task<IEnumerable<UserPlatform>> GetUserPlatformsAsync()
        {
            return await context.UserPlatforms.ToListAsync();
        }

        /// <inheritdoc cref="IUserPlatformRepository.GetUserPlatformsByAsync"/>
        public async Task<IEnumerable<UserPlatform>> GetUserPlatformsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<UserPlatform> query = context.UserPlatforms;

            foreach (KeyValuePair<string, string> filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(UserPlatform.Id) when Guid.TryParse(filter.Value, out Guid outId):
                        query = query.Where(u => u.Id == outId);
                        break;

                    case nameof(UserPlatform.UserId) when Guid.TryParse(filter.Value, out Guid outUserId):
                        query = query.Where(u => u.UserId == outUserId);
                        break;

                    case nameof(UserPlatform.PlatformId) when Guid.TryParse(filter.Value, out Guid outPlatformId):
                        query = query.Where(u => u.PlatformId == outPlatformId);
                        break;

                    default:
                        break;
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IUserPlatformRepository.AddUserPlatformAsync"/>
        public async Task AddUserPlatformAsync(UserPlatform UserPlatform)
        {
            _ = context.UserPlatforms.Add(UserPlatform);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserPlatformRepository.UpdateUserPlatformAsync"/>
        public async Task UpdateUserPlatformAsync(UserPlatform UserPlatform)
        {
            _ = context.UserPlatforms.Update(UserPlatform);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserPlatformRepository.DeleteUserPlatformAsync"/>
        public async Task DeleteUserPlatformAsync(UserPlatform UserPlatform)
        {
            _ = context.UserPlatforms.Remove(UserPlatform);
            _ = await context.SaveChangesAsync();
        }
    }
}
