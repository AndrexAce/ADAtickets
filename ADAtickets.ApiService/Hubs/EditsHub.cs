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

using Microsoft.AspNetCore.SignalR;

namespace ADAtickets.ApiService.Hubs;

/// <summary>
///     SignalR hub managing real-time communincation with clients when edits are created, updated or deleted.
/// </summary>
public sealed class EditsHub : Hub
{
    /// <summary>
    ///     Inserts the user in a personal group based on the ticket identifier to send targeted notifications.
    /// </summary>
    /// <param name="ticketId">Id of the ticket the user wants to join the group of.</param>
    /// <returns>A <see cref="Task"/> running the operation.</returns>
    public async Task JoinPersonalGroup(string ticketId)
    {
        if (!string.IsNullOrEmpty(ticketId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket_{ticketId}");
        }
    }
}