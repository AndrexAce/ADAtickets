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
    /// Implements the methods to manage the <see cref="Notification"/> entities in the underlying database.
    /// </summary>
    class NotificationRepository(ADAticketsDbContext context) : INotificationRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="INotificationRepository.GetNotificationByIdAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity was not found.</exception>
        public async Task<Notification> GetNotificationByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id) ?? throw new InvalidOperationException($"Entity of type {typeof(Notification)} with id {id} was not found.");
        }

        /// <inheritdoc cref="INotificationRepository.GetNotificationsAsync"/>
        public async IAsyncEnumerable<Notification> GetNotificationsAsync()
        {
            await foreach (var notification in _context.Notifications.AsAsyncEnumerable())
            {
                yield return notification;
            }
        }

        /// <inheritdoc cref="INotificationRepository.AddNotificationAsync(Notification)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="INotificationRepository.UpdateNotificationAsync(Notification)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflict.</exception>
        public async Task UpdateNotificationAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="INotificationRepository.DeleteNotificationAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>
        public async Task DeleteNotificationAsync(Guid id)
        {
            if (await _context.Notifications.FindAsync(id) is not Notification notification)
                throw new InvalidOperationException($"Entity of type {typeof(Notification)} with id {id} was not found.");
            _context.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
