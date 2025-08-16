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
namespace ADAtickets.Web.Components.Utilities;

/// <summary>
///     Service used to manage ticket refresh requests across different components.
/// </summary>
class TicketRefreshService
{
    /// <summary>
    ///     Incapsulates the method to be called when a refresh is requested.
    /// </summary>
    public event Action? RefreshRequested;

    /// <summary>
    ///     Request the list of tickets to be refreshed with all the filters and sortings applied.
    /// </summary>
    public void RequestRefresh()
    {
        RefreshRequested?.Invoke();
    }
}