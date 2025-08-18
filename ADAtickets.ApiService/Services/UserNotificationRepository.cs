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

namespace ADAtickets.ApiService.Services;

/// <summary>
///     Implements the methods to manage the <see cref="UserNotification" /> entities in the underlying database.
/// </summary>
internal sealed class UserNotificationRepository(ADAticketsDbContext context) : IUserNotificationRepository
{
    /// <inheritdoc cref="IUserNotificationRepository.GetUserNotificationByIdAsync" />
    public async Task<UserNotification?> GetUserNotificationByIdAsync(Guid id)
    {
        return await context.UserNotifications.Include(un => un.Notification)
            .ThenInclude(n => n.User)
            .Include(un => un.Notification)
            .ThenInclude(n => n.Ticket)
            .FirstOrDefaultAsync(un => un.Id == id);
    }

    /// <inheritdoc cref="IUserNotificationRepository.GetUserNotificationsAsync" />
    public async Task<IEnumerable<UserNotification>> GetUserNotificationsAsync()
    {
        return await context.UserNotifications.Include(un => un.Notification)
            .ThenInclude(n => n.User)
            .Include(un => un.Notification)
            .ThenInclude(n => n.Ticket)
            .ToListAsync();
    }

    /// <inheritdoc cref="IUserNotificationRepository.GetUserNotificationsByAsync" />
    public async Task<IEnumerable<UserNotification>> GetUserNotificationsByAsync(
        IEnumerable<KeyValuePair<string, string>> filters)
    {
        IQueryable<UserNotification> query = context.UserNotifications.Include(un => un.Notification)
            .ThenInclude(n => n.User)
            .Include(un => un.Notification)
            .ThenInclude(n => n.Ticket);

        foreach (var filter in filters)
            switch (filter.Key.Pascalize())
            {
                case nameof(UserNotification.Id) when Guid.TryParse(filter.Value, out var outId):
                    query = query.Where(u => u.Id == outId);
                    break;

                case nameof(UserNotification.ReceiverUserId)
                    when Guid.TryParse(filter.Value, out var outReceiverUserId):
                    query = query.Where(u => u.ReceiverUserId == outReceiverUserId);
                    break;

                case nameof(UserNotification.NotificationId)
                    when Guid.TryParse(filter.Value, out var outNotificationId):
                    query = query.Where(u => u.NotificationId == outNotificationId);
                    break;

                case nameof(UserNotification.IsRead) when bool.TryParse(filter.Value, out var outIsRead):
                    query = query.Where(notification => notification.IsRead == outIsRead);
                    break;
            }

        return await query.ToListAsync();
    }

    /// <inheritdoc cref="IUserNotificationRepository.AddUserNotificationAsync" />
    public async Task AddUserNotificationAsync(UserNotification UserNotification)
    {
        _ = context.UserNotifications.Add(UserNotification);
        _ = await context.SaveChangesAsync();
    }

    /// <inheritdoc cref="IUserNotificationRepository.UpdateUserNotificationAsync" />
    public async Task UpdateUserNotificationAsync(UserNotification UserNotification)
    {
        _ = context.UserNotifications.Update(UserNotification);
        _ = await context.SaveChangesAsync();
    }

    /// <inheritdoc cref="IUserNotificationRepository.DeleteUserNotificationAsync" />
    public async Task DeleteUserNotificationAsync(UserNotification UserNotification)
    {
        _ = context.UserNotifications.Remove(UserNotification);
        _ = await context.SaveChangesAsync();
    }
}