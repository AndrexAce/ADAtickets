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
using System.Text.Json.Serialization;

namespace ADAtickets.Shared.Dtos.Responses;

/// <summary>
///     <para>Represents a user of the system.</para>
///     <para>It is a simplified version of the <see cref="User" /> class, used for data transfer to the client.</para>
/// </summary>
public sealed record UserResponseDto : ResponseDto
{
    /// <summary>
    ///     The email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    ///     The username of the user.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    ///     The name of the user.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     The surname of the user.
    /// </summary>
    public string Surname { get; init; } = string.Empty;

    /// <summary>
    ///     Whether the user enabled the reception of external notifications via email.
    /// </summary>
    public bool AreEmailNotificationsEnabled { get; init; } = false;

    /// <summary>
    ///     The role of the user in the system.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserType Type { get; init; } = UserType.User;

    /// <summary>
    ///     The collection of ids of tickets created by the user (if they are a user, otherwise it must be empty).
    /// </summary>
    public List<Guid> CreatedTickets { get; init; } = [];

    /// <summary>
    ///     The collection of ids of tickets assigned to the user (if they are an operator, otherwise it must be empty).
    /// </summary>
    public List<Guid> AssignedTickets { get; init; } = [];

    /// <summary>
    ///     The collection of ids of replies sent by the user to any ticket.
    /// </summary>
    public List<Guid> Replies { get; init; } = [];

    /// <summary>
    ///     The collection of ids of edits made by the user to any ticket.
    /// </summary>
    public List<Guid> Edits { get; init; } = [];

    /// <summary>
    ///     The collection of ids of platforms the user prefers (if they are an operator, otherwise it must be empty).
    /// </summary>
    public List<Guid> PreferredPlatforms { get; init; } = [];

    /// <summary>
    ///     The collection of ids of notifications the user triggered the sending of.
    /// </summary>
    public List<Guid> SentNotifications { get; init; } = [];

    /// <summary>
    ///     The collection of ids of notifications the user received.
    /// </summary>
    public List<Guid> ReceivedNotifications { get; init; } = [];
}