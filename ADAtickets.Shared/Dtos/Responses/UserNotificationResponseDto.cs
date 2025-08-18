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

namespace ADAtickets.Shared.Dtos.Responses;

/// <summary>
///     <para>Represents the link between a user and their received notification.</para>
///     <para>
///         It is a simplified version of the <see cref="UserNotification" /> class, used for data transfer to the
///         client.
///     </para>
/// </summary>
public sealed record UserNotificationResponseDto : ResponseDto
{
    /// <summary>
    ///     The unique identifier of the user.
    /// </summary>
    public Guid ReceiverUserId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The unique identifier of the notification.
    /// </summary>
    public Guid NotificationId { get; init; } = Guid.Empty;

    /// <summary>
    ///     Whether the notification has been read by the user.
    /// </summary>
    public bool IsRead { get; init; } = false;

    /// <summary>
    ///     The text of the notification (from related entity <see cref="Notification" />).
    /// </summary>
    [ValueFromRelationship]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    ///     The sending date and time of the notification (from related entity <see cref="Notification" />).
    /// </summary>
    [ValueFromRelationship]
    public DateTimeOffset SendDateTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     The name of the user responsible for the notification (from related entities <see cref="Notification" /> and <see cref="User" />).
    /// </summary>
    [ValueFromRelationship]
    public string SenderName { get; init; } = string.Empty;

    /// <summary>
    ///     The ID of the ticket linked to the notification (from related entities <see cref="Notification" /> and <see cref="Ticket" />).
    /// </summary>
    [ValueFromRelationship]
    public Guid TicketId { get; init; } = Guid.Empty;

    /// <summary>
    ///     The title of the ticket linked to the notification (from related entities <see cref="Notification" /> and <see cref="Ticket" />).
    /// </summary>
    [ValueFromRelationship]
    public string TicketTitle { get; init; } = string.Empty;

}