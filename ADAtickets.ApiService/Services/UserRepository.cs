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

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="User"/> entities in the underlying database.
    /// </summary>
    class UserRepository(ADAticketsDbContext context) : IUserRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IUserRepository.GetUserByIdAsync"/>
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc cref="IUserRepository.GetUsersAsync"/>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc cref="IUserRepository.GetUsersByAsync"/>
        public async Task<IEnumerable<User>> GetUsersByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<User> query = _context.Users;

            foreach (var filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(User.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(u => u.Id == outGuid);
                        break;

                    case nameof(User.Email):
                        query = query.Where(u => u.Email.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case nameof(User.Name):
                        query = query.Where(u => u.Name.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case nameof(User.Surname):
                        query = query.Where(u => u.Surname.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case nameof(User.PhoneNumber):
                        query = query.Where(u => u.PhoneNumber.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case nameof(User.AreEmailNotificationsEnabled) when bool.TryParse(filter.Value, out bool outBool):
                        query = query.Where(u => u.AreEmailNotificationsEnabled == outBool);
                        break;

                    case nameof(User.ArePhoneNotificationsEnabled) when bool.TryParse(filter.Value, out bool outBool):
                        query = query.Where(u => u.ArePhoneNotificationsEnabled == outBool);
                        break;

                    case nameof(User.Type) when Enum.TryParse(filter.Value, true, out UserType outUserType):
                        query = query.Where(u => u.Type == outUserType);
                        break;

                    case nameof(User.MicrosoftAccountId):
                        query = query.Where(u => u.MicrosoftAccountId != null && u.MicrosoftAccountId.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IUserRepository.AddUserAsync"/>
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.UpdateUserAsync"/>
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.DeleteUserAsync"/>
        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
