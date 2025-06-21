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

namespace ADAtickets.Shared.Dtos.Requests
{
    /// <summary>
    /// <para>Represents an attachment associated with a ticket.</para>
    /// <para>It is a simplified version of the <see cref="Attachment"/> class, used for data transfer to the server.</para>
    /// </summary>
    public class AttachmentRequestDto : RequestDto
    {
        /// <summary>
        /// The name of the attachment.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The id of the ticket this attachment is related to.
        /// </summary>
        [Required]
        public Guid TicketId { get; set; } = Guid.Empty;

        /// <summary>
        /// The content of the attachment.
        /// </summary>
        [Required]
        public byte[] Content { get; set; } = [];
    }
}
