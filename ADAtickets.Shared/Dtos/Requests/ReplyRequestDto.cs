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

using System.ComponentModel.DataAnnotations;

namespace ADAtickets.Shared.Dtos.Requests;

/// <summary>
///     <para>Represents a reply in a ticket comment thread.</para>
///     <para>It is a simplified version of the <see cref="Reply" /> class, used for data transfer to the server.</para>
/// </summary>
public sealed class ReplyRequestDto : RequestDto
{
    /// <summary>
    ///     The date and time when the reply was sent.
    /// </summary>
    [Required]
    public DateTimeOffset ReplyDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The message written in the reply.
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The id of the user who sent the reply.
    /// </summary>
    [Required]
    public Guid AuthorUserId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The id of the ticket this reply was sent to.
    /// </summary>
    [Required]
    public Guid TicketId { get; set; } = Guid.Empty;
}