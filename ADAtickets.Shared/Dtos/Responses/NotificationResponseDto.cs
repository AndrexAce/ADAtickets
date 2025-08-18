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

namespace ADAtickets.Shared.Dtos.Responses;

/// <summary>
///     <para>
///         Represents a notification sent to a user triggered by an action like ticket modification, reply or
///         assignment.
///     </para>
///     <para>It is a simplified version of the <see cref="Notification" /> class, used for data transfer to the client.</para>
/// </summary>
public sealed record NotificationResponseDto : ResponseDto
{
    /// <summary>
    ///     The date and time when the notification was sent.
    /// </summary>
    public DateTimeOffset SendDateTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The message the notification comes with.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     The id of the ticket this notification is related to.
    /// </summary>
    public Guid TicketId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The id of the user this notification is related to.
    /// </summary>
    public Guid UserId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The collection of ids of the users the notification was sent to.
    /// </summary>
    public List<Guid> Recipients { get; init; } = [];
}