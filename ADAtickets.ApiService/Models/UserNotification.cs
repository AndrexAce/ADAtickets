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
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents the link between users and sent notifications.
    /// </summary>
    [PrimaryKey(nameof(ReceiverUserEmail), nameof(NotificationId))]
    internal class UserNotification : EntityBase
    {
        /// <value>
        /// Gets or sets the email of the user who received the notification.
        /// </value>
        [MaxLength(254)]
        internal string ReceiverUserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the user who received the notification.
        /// </value>
        internal User ReceiverUser { get; set; } = new User();

        /// <value>
        /// Gets or sets the unique identifier of the sent notification.
        /// </value>
        internal Guid NotificationId { get; set; } = Guid.Empty;
        /// <value>
        /// Gets or sets the notification that was sent.
        /// </value>
        internal Notification Notification { get; set; } = new Notification();
    }
}
