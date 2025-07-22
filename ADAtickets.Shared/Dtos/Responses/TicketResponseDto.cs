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
using ADAtickets.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ADAtickets.Shared.Dtos.Responses
{
    /// <summary>
    /// <para>Represents a ticket sent by a user to the system.</para>
    /// <para>It is a simplified version of the <see cref="Ticket"/> class, used for data transfer to the client.</para>
    /// </summary>
    public sealed class TicketResponseDto : ResponseDto
    {
        /// <summary>
        /// The type of user request bound to the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TicketType Type { get; init; } = TicketType.Bug;

        /// <summary>
        /// The date and time when the ticket was created.
        /// </summary>
        public DateTimeOffset CreationDateTime { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The title of the ticket, a brief recap of the issue.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// The description of the ticket, a detailed description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// The urgency of the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Priority Priority { get; init; } = Priority.Low;

        /// <summary>
        /// The status of the ticket.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; init; } = Status.Unassigned;

        /// <summary>
        /// The id of the work item the ticket is related to in Azure DevOps.
        /// </summary>
        public int WorkItemId { get; init; } = 0;

        /// <summary>
        /// The id of the platform the ticket is related to.
        /// </summary>
        public Guid PlatformId { get; init; } = Guid.Empty;

        /// <summary>
        /// The id of the user who created the ticket.
        /// </summary>
        public Guid CreatorUserId { get; init; } = Guid.Empty;

        /// <summary>
        /// The name of the user who created the ticket (from related entity <see cref="User"/>).
        /// </summary>
        public string CreatorName { get; init; } = string.Empty;

        /// <summary>
        /// The id of the operator assigned to the ticket.
        /// </summary>
        public Guid? OperatorUserId { get; init; } = null;

        /// <summary>
        /// The collection of ids of edits made to the ticket.
        /// </summary>
        public List<Guid> Edits { get; init; } = [];

        /// <summary>
        /// The last update date of the ticket (from related entity <see cref="Edit"/>).
        /// </summary>
        public DateTimeOffset LastUpdateDateTime { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The collection of ids of replies sent to the ticket.
        /// </summary>
        public List<Guid> Replies { get; init; } = [];

        /// <summary>
        /// The collection of ids of attachments attached to the ticket.
        /// </summary>

        public List<Guid> Attachments { get; init; } = [];

        /// <summary>
        /// The collection of ids of notifications related to the ticket.
        /// </summary>
        public List<Guid> Notifications { get; init; } = [];
    }
}
