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
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents a notification sent to a user triggered by an action like ticket modification, reply or assignment.
    /// </summary>
    sealed class Notification : EntityBase
    {
        /// <value>
        /// Gets or sets the unique identifier of the notification.
        /// </value>
        internal Guid Id { get; set; } = Guid.NewGuid();
        /// <value>
        /// Gets or sets the date and time when the notification was sent.
        /// </value>
        internal DateTimeOffset SendDateTime { get; } = DateTimeOffset.Now;
        /// <value>
        /// Gets or sets the message the notification cames with.
        /// </value>
        [MaxLength(200)]
        internal string Message { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets wheter the notification has been read by the user.
        /// </value>
        internal bool IsRead { get; set; } = false;

        /// <value>
        /// Gets or sets the id of the ticket this notification is related to.
        /// </value>
        internal Guid TicketId { get; set; } = Guid.Empty;
        /// <value>
        /// Gets or sets the ticket this notification is related to.
        /// </value>
        internal Ticket Ticket { get; set; } = new Ticket();

        /// <value>
        /// Gets or sets the id of the user this notification is related to.
        /// </value>
        [MaxLength(254)]
        internal string UserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the user this notification is related to.
        /// </value>
        internal User User { get; set; } = new User();

        /// <value>
        /// Gets the collection of the sent notifications and the user they were sent to.
        /// </value>
        internal ICollection<UserNotification> SentNotifications { get; } = [];
    }
}
