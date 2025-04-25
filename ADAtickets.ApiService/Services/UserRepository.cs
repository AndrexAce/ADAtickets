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
    /// Implements the methods to manage the <see cref="User"/> entities in the underlying database.
    /// </summary>
    class UserRepository(ADAticketsDbContext context) : IUserRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IUserRepository.GetUserByEmailAsync(string)"/>
        /// <exception cref="InvalidOperationException">When the entity was not found.</exception>

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FindAsync(email) ?? throw new InvalidOperationException($"Entity of type {typeof(User)} with email {email} was not found.");
        }

        /// <inheritdoc cref="IUserRepository.GetUsersAsync"/>
        public async IAsyncEnumerable<User> GetUsersAsync()
        {
            await foreach (var user in _context.Users.AsAsyncEnumerable())
            {
                yield return user;
            }
        }

        /// <inheritdoc cref="IUserRepository.AddUserAsync(User)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.UpdateUserAsync(User)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflict.</exception>

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IUserRepository.DeleteUserAsync(string)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>

        public async Task DeleteUserAsync(string email)
        {
            if (await _context.Users.FindAsync(email) is not User user)
                throw new InvalidOperationException($"Entity of type {typeof(User)} with email {email} was not found.");
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
