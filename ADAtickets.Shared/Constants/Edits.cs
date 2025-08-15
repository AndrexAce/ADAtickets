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

namespace ADAtickets.Shared.Constants;

/// <summary>
///     Represents the various messages attached to edits that should be translated on the web app.
/// </summary>
internal static class Edits
{
    /// <summary>
    ///     The message used when the ticket is first created.
    /// </summary>
    public const string TicketCreated = "TICKET_CREATED_EDIT";

    /// <summary>
    ///     The message used when the ticket is assigned to an operator by the system.
    /// </summary>
    public const string TicketAutoAssigned = "TICKET_AUTO_ASSIGNED_EDIT";

    /// <summary>
    ///     The message used when the ticket is edited by the user or operator.
    /// </summary>
    public const string TicketEdited = "TICKET_EDITED_EDIT";

    /// <summary>
    ///     The message used when the ticket is assigned to an operator.
    /// </summary>
    public const string TicketAssigned = "TICKET_ASSIGNED_EDIT";

    /// <summary>
    ///     The message used when the ticket is unassigned from an operator.
    /// </summary>
    public const string TicketUnassigned = "TICKET_UNASSIGNED_EDIT";
}