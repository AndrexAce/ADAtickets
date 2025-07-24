﻿/*
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
    /// Implements the methods to manage the <see cref="User"/> entities in the underlying database.
    /// </summary>
    internal class UserRepository(ADAticketsDbContext context) : IUserRepository
    {
        /// <inheritdoc cref="IUserRepository.GetUserByIdAsync"/>
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await context.Users.FindAsync(id);
        }

        /// <inheritdoc cref="IUserRepository.GetUsersAsync"/>
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await context.Users.ToListAsync();
        }

        /// <inheritdoc cref="IUserRepository.GetUsersByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<User>> GetUsersByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<User> query = context.Users;

            foreach (KeyValuePair<string, string> filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(User.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(u => u.Id == outGuid);
                        break;

                    case nameof(User.Email):
                        query = query.Where(u => u.Email.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    case nameof(User.Name):
                        query = query.Where(u => u.Name.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    case nameof(User.Surname):
                        query = query.Where(u => u.Surname.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    case nameof(User.PhoneNumber):
                        query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(filter.Value));
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

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IUserRepository.AddUserAsync"/>
        public async Task AddUserAsync(User user)
        {
            _ = context.Users.Add(user);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.UpdateUserAsync"/>
        public async Task UpdateUserAsync(User user)
        {
            _ = context.Users.Update(user);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.DeleteUserAsync"/>
        public async Task DeleteUserAsync(User user)
        {
            _ = context.Users.Remove(user);
            _ = await context.SaveChangesAsync();
        }
    }
}
