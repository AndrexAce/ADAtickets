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
    /// Represents a modification made to a ticket, either by a user or by the system.
    /// </summary>
    sealed class Edit : EntityBase
    {
        /// <value>
        /// Gets or sets the unique identifier of the edit.
        /// </value>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Gets or sets the date and time when the edit was made.
        /// </summary>
        public DateTimeOffset EditDateTime { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Gets or sets the message the edit comes with.
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the status the ticket was in before the edit.
        /// </summary>
        public Status OldStatus { get; set; } = Status.UNASSIGNED;
        /// <summary>
        /// Gets or sets the status the ticket will be after the edit.
        /// </summary>
        public Status NewStatus { get; set; } = Status.UNASSIGNED;

        /// <value>
        /// Gets or sets the id of the ticket this edit is related to.
        /// </value>
        public Guid TicketId { get; set; } = Guid.Empty;
        /// <value>
        /// Gets or sets the the ticket this edit is related to.
        /// </value>
        public Ticket Ticket { get; set; } = new Ticket();

        /// <summary>
        /// Gets or sets the email of the user who made the edit.
        /// </summary>
        [MaxLength(254)]
        public string UserEmail { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the user who made the edit.
        /// </summary>
        public User User { get; set; } = new User();
    }
}
