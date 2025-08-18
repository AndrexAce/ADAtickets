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

using ADAtickets.Client;
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;

namespace ADAtickets.Web.Components.Utilities;

/// <summary>
///     Service managing shared notification state between components with thread-safe operations.
/// </summary>
internal sealed class NotificationsManager
{
    private IEnumerable<UserNotificationResponseDto> userNotifications = [];
    private readonly Lock cLock = new();

    /// <summary>
    ///     Event triggered when the collection of <see cref="UserNotification"/> collection changes.
    /// </summary>
    public event Action? OnUserNotificationsChanged;

    /// <summary>
    ///     Thread-safe access to the current <see cref="UserNotification"/> collection.
    /// </summary>
    public IEnumerable<UserNotificationResponseDto> UserNotifications
    {
        get
        {
            using (cLock.EnterScope())
            {
                // Return a copy to prevent external modification.
                return [.. userNotifications];
            }
        }

        private set
        {
            using (cLock.EnterScope())
            {
                userNotifications = value;
            }

            // Notify subscribers that the userNotification collection has changed.
            OnUserNotificationsChanged?.Invoke();
        }
    }

    /// <summary>
    ///     Updates the <see cref="UserNotification"/> collection in a thread-safe manner.
    /// </summary>
    /// <param name="notifications">The new <see cref="UserNotification"/> collection.</param>
    public async Task UpdateUserNotificationsAsync(IEnumerable<UserNotificationResponseDto> userNotifications)
    {
        await Task.Run(() =>
        {
            UserNotifications = userNotifications.OrderByDescending(un => un.SendDateTime);
        });
    }

    /// <summary>
    ///     Marks a specific <see cref="UserNotification"/> as read both locally and on the server.
    /// </summary>
    /// <param name="userNotificationId">The ID of the <see cref="UserNotification"/> to mark as read.</param>
    /// <param name="client">The client to use for server communication.</param>
    public async Task MarkAsReadAsync(Guid userNotificationId, UserNotificationsClient client)
    {
        UserNotificationResponseDto? selectedUserNotification = null;
        IEnumerable<UserNotificationResponseDto> originalList;

        using (cLock.EnterScope())
        {
            // Store the original list to revert if needed.
            originalList = [.. userNotifications];

            // Create a list to edit userNotifications.
            var userNotificationsList = userNotifications.ToList();

            var index = userNotificationsList.FindIndex(n => n.Id == userNotificationId);

            if (index >= 0)
            {
                // Mark the userNotification as read in the local list.
                selectedUserNotification = userNotificationsList[index];
                userNotificationsList[index] = selectedUserNotification with { IsRead = true };
                userNotifications = userNotificationsList;
            }
        }

        // Trigger immediate UI update.
        OnUserNotificationsChanged?.Invoke();

        try
        {
            if (selectedUserNotification is null)
            {
                throw new InvalidOperationException("The specified userNotification does not exist.");
            }

            // Update on server if the userNotification exists.
            await client.PutAsync(userNotificationId, new UserNotificationRequestDto
            {
                IsRead = true,
                NotificationId = selectedUserNotification.NotificationId,
                ReceiverUserId = selectedUserNotification.ReceiverUserId
            });
        }
        catch
        {
            using (cLock.EnterScope())
            {
                // If server update fails, revert the local change.
                userNotifications = originalList;
            }

            // Notify subscribers that the userNotification collection has changed.
            OnUserNotificationsChanged?.Invoke();
        }
    }

    /// <summary>
    ///     Deletes a specific <see cref="UserNotification"/> both locally and on the server.
    /// </summary>
    /// <param name="userNotificationId">The ID of the <see cref="UserNotification"/> to delete.</param>
    /// <param name="client">The client to use for server communication.</param>
    public async Task DeleteAsync(Guid userNotificationId, UserNotificationsClient client)
    {
        UserNotificationResponseDto? selectedUserNotification = null;
        IEnumerable<UserNotificationResponseDto> originalList;

        using (cLock.EnterScope())
        {
            // Store the original list to revert if needed.
            originalList = [.. userNotifications];

            // Create a list to edit userNotifications.
            var userNotificationsList = userNotifications.ToList();

            var index = userNotificationsList.FindIndex(n => n.Id == userNotificationId);

            if (index >= 0)
            {
                // Remove the userNotification from the local list.
                selectedUserNotification = userNotificationsList[index];
                userNotificationsList.RemoveAt(index);
                userNotifications = userNotificationsList;
            }
        }

        // Trigger immediate UI update.
        OnUserNotificationsChanged?.Invoke();

        try
        {
            if (selectedUserNotification is null)
            {
                throw new InvalidOperationException("The specified userNotification does not exist.");
            }

            // Delete on server if the userNotification exists.
            await client.DeleteAsync(userNotificationId);
        }
        catch
        {
            using (cLock.EnterScope())
            {
                // If server update fails, revert the local change.
                userNotifications = originalList;
            }

            // Notify subscribers that the userNotification collection has changed.
            OnUserNotificationsChanged?.Invoke();
        }
    }
}