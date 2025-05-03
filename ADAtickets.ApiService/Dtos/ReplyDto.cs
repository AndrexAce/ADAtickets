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

namespace ADAtickets.ApiService.Dtos
{
    /// <summary>
    /// <para>Represents a reply in a ticket comment thread.</para>
    /// <para>It is a simplified version of the <see cref="Reply"/> class, used for data transfer.</para>
    /// </summary>
    public sealed class ReplyDto
    {
        /// <summary>
        /// The unique identifier of the reply.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The date and time when the reply was sent.
        /// </summary>
        public DateTimeOffset ReplyDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// The message written in the reply.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The id of the user who sent the reply.
        /// </summary>
        public Guid AuthorUserId { get; set; } = Guid.Empty;

        /// <summary>
        /// The id of the ticket this reply was sent to.
        /// </summary>
        public Guid TicketId { get; set; } = Guid.Empty;
    }
}
