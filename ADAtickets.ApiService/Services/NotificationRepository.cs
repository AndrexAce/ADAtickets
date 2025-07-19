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
using System.Globalization;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Notification"/> entities in the underlying database.
    /// </summary>
    internal class NotificationRepository(ADAticketsDbContext context) : INotificationRepository
    {
        /// <inheritdoc cref="INotificationRepository.GetNotificationByIdAsync"/>
        public async Task<Notification?> GetNotificationByIdAsync(Guid id)
        {
            return await context.Notifications.FindAsync(id);
        }

        /// <inheritdoc cref="INotificationRepository.GetNotificationsAsync"/>
        public async Task<IEnumerable<Notification>> GetNotificationsAsync()
        {
            return await context.Notifications.ToListAsync();
        }

        /// <inheritdoc cref="INotificationRepository.GetNotificationsByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<Notification>> GetNotificationsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Notification> query = context.Notifications;

            foreach (KeyValuePair<string, string> filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Notification.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(notification => notification.Id == outGuid);
                        break;

                    case nameof(Notification.TicketId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(notification => notification.TicketId == outGuid);
                        break;

                    case nameof(Notification.UserId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(notification => notification.UserId == outGuid);
                        break;

                    case nameof(Notification.IsRead) when bool.TryParse(filter.Value, out bool outBool):
                        query = query.Where(notification => notification.IsRead == outBool);
                        break;

                    case nameof(Notification.SendDateTime) when DateTimeOffset.TryParse(filter.Value, CultureInfo.InvariantCulture, out DateTimeOffset outDateTimeOffset):
                        query = query.Where(notification => notification.SendDateTime.Date == outDateTimeOffset.Date);
                        break;

                    case nameof(Notification.Message):
                        query = query.Where(notification => notification.Message.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="INotificationRepository.AddNotificationAsync"/>
        public async Task AddNotificationAsync(Notification notification)
        {
            _ = context.Notifications.Add(notification);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="INotificationRepository.UpdateNotificationAsync"/>
        public async Task UpdateNotificationAsync(Notification notification)
        {
            _ = context.Notifications.Update(notification);
            _ = await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="INotificationRepository.DeleteNotificationAsync"/>
        public async Task DeleteNotificationAsync(Notification notification)
        {
            _ = context.Notifications.Remove(notification);
            _ = await context.SaveChangesAsync();
        }
    }
}
