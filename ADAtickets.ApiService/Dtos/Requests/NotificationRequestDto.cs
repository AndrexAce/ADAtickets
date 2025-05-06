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
using System.ComponentModel.DataAnnotations;

namespace ADAtickets.ApiService.Dtos.Requests
{
    /// <summary>
    /// <para>Represents a notification sent to a user triggered by an action like ticket modification, reply or assignment.</para>
    /// <para>It is a simplified version of the <see cref="Notification"/> class, used for data transfer to the server.</para>
    /// </summary>
    public sealed class NotificationRequestDto
    {
        /// <summary>
        /// The date and time when the notification was sent.
        /// </summary>
        [Required]
        public DateTimeOffset SendDateTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The message the notification comes with.
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Whether the notification has been read by the user.
        /// </summary>
        [Required]
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// The id of the ticket this notification is related to.
        /// </summary>
        [Required]
        public Guid TicketId { get; set; } = Guid.Empty;

        /// <summary>
        /// The id of the user this notification is related to.
        /// </summary>
        [Required]
        public Guid UserId { get; set; } = Guid.Empty;
    }
}
