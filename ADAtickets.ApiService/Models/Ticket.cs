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
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents a ticket sent by a user to the system.
    /// </summary>
    sealed class Ticket : EntityBase
    {
        /// <value>
        /// Gets or sets the unique identifier of the ticket.
        /// </value>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <value>
        /// Gets or sets the type of user request bound to the ticket.
        /// </value>
        public TicketType Type { get; set; } = TicketType.BUG;
        /// <value>
        /// Gets or sets the date and time when the ticket was created.
        /// </value>
        public DateTimeOffset CreationDateTime { get; } = DateTimeOffset.Now;
        [MaxLength(50)]
        /// <value>
        /// Gets or sets the title of the ticket, a brief recap of the issue.
        /// </value>
        public string Title { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the description of the ticket, a detailed description of the issue.
        /// </value>
        [MaxLength(5000)]
        public string Description { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the urgency of the ticket.
        /// </value>
        public Priority Priority { get; set; } = Priority.LOW;
        /// <value>
        /// Gets or sets the status of the ticket.
        /// </value>
        public Status Status { get; set; } = Status.UNASSIGNED;
        /// <value>
        /// Gets or sets the id of the work item the ticket is related to in Azure DevOps.
        /// </value>
        public int WorkItemId { get; set; } = 0;

        /// <value>
        /// Gets or sets the name of the platform the ticket is related to.
        /// </value>
        [MaxLength(4000)]
        public string PlatformName { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the platform the ticket is related to.
        /// </value>
        public Platform Platform { get; set; } = new Platform();

        /// <value>
        /// Gets or sets the email of the user who created the ticket.
        /// </value>
        [ForeignKey(nameof(CreatorUser))]
        [MaxLength(254)]
        public string CreatorUserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the user who created the ticket.
        /// </value>
        public User CreatorUser { get; set; } = new User();

        /// <value>
        /// Gets or sets the email of the operator assigned to the ticket.
        /// </value>
        [ForeignKey(nameof(OperatorUser))]
        [MaxLength(254)]
        public string OperatorUserEmail { get; set; } = string.Empty;
        /// <value>
        /// Gets or sets the operator assigned to the ticket.
        /// </value>
        public User OperatorUser { get; set; } = new User();

        /// <value>
        /// Gets the collection of edits made to the ticket.
        /// </value>
        public ICollection<Edit> Edits { get; } = [];

        /// <value>
        /// Gets the collection of replies sent to the ticket.
        /// </value>
        public ICollection<Reply> Replies { get; } = [];

        /// <value>
        /// Gets the collection of attachments attached to the ticket.
        /// </value>
        public ICollection<Attachment> Attachments { get; } = [];

        /// <value>
        /// Gets the collection of notifications related to the ticket.
        /// </value>
        public ICollection<Notification> Notifications { get; } = [];
    }
}
