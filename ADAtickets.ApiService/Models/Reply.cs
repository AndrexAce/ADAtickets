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
    /// Represents a reply in a ticket comment thread.
    /// </summary>
    sealed class Reply : EntityBase
    {
        /// <value>
        /// Gets or sets the unique identifier of the reply.
        /// </value>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <value>
        /// Gets or sets the date and time when the reply was sent.
        /// </value>
        public DateTimeOffset ReplyDateTime { get; set; } = DateTimeOffset.Now;
        /// <value>
        /// Gets or sets the message written in the reply.
        /// </value>
        [MaxLength(5000)]
        public string Message { get; set; } = string.Empty;

        /// <value>
        /// Gets or sets the email of the user who sent the reply.
        /// </value>
        [MaxLength(254)]
        public string AuthorUserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the user who sent the reply.
        /// </value>
        public User AuthorUser { get; set; } = new User();

        /// <value>
        /// Gets or sets the id of the ticket this reply was sent to.
        /// </value>
        public Guid TicketId { get; set; } = Guid.Empty;
        /// <value>
        /// Gets or sets the ticket this reply was sent to.
        /// </value>
        public Ticket Ticket { get; set; } = new Ticket();
    }
}
