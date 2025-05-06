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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADAtickets.ApiService.Dtos.Responses
{
    /// <summary>
    /// <para>Represents a ticket sent by a user to the system.</para>
    /// <para>It is a simplified version of the <see cref="Ticket"/> class, used for data transfer to the client.</para>
    /// </summary>
    public sealed class TicketResponseDto
    {
        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The type of user request bound to the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TicketType Type { get; set; } = TicketType.Bug;

        /// <summary>
        /// The date and time when the ticket was created.
        /// </summary>
        public DateTimeOffset CreationDateTime { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The title of the ticket, a brief recap of the issue.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket, a detailed description of the issue.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The urgency of the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Priority Priority { get; set; } = Priority.Low;

        /// <summary>
        /// The status of the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; } = Status.Unassigned;

        /// <summary>
        /// The id of the work item the ticket is related to in Azure DevOps.
        /// </summary>
        public int WorkItemId { get; set; } = 0;

        /// <summary>
        /// The id of the platform the ticket is related to.
        /// </summary>
        public Guid PlatformId { get; set; } = Guid.Empty;

        /// <summary>
        /// The id of the user who created the ticket.
        /// </summary>
        public Guid CreatorUserId { get; set; } = Guid.Empty;

        /// <summary>
        /// The id of the operator assigned to the ticket.
        /// </summary>
        public Guid? OperatorUserId { get; set; } = null;

        /// <summary>
        /// The collection of ids of edits made to the ticket.
        /// </summary>
        public ICollection<Guid> Edits { get; } = [];

        /// <summary>
        /// The collection of ids of replies sent to the ticket.
        /// </summary>
        public ICollection<Guid> Replies { get; } = [];

        /// <summary>
        /// The collection of ids of attachments attached to the ticket.
        /// </summary>
        public ICollection<Guid> Attachments { get; } = [];

        /// <summary>
        /// The collection of ids of notifications related to the ticket.
        /// </summary>
        public ICollection<Guid> Notifications { get; } = [];
    }
}
