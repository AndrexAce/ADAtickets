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
namespace ADAtickets.Shared.Constants
{
    /// <summary>
    /// Represents the various messages attached to notifications that should be translated on the web app.
    /// </summary>
    internal static class Notifications
    {
        /// <summary>
        /// The message sent to the operators when a ticket is created by a user.
        /// </summary>
        public const string TicketCreated = "TICKET_CREATED_NOTIFICATION";

        /// <summary>
        /// The message sent to the operator when a ticket is automatically assigned to them by the system.
        /// </summary>
        public const string TicketAssignedToYouBySystem = "TICKET_ASSIGNED_TO_YOU_BY_SYSTEM_NOTIFICATION";

        /// <summary>
        /// The message sent to the user when a ticket is assigned to an operator.
        /// </summary>
        public const string TicketAssigned = "TICKET_ASSIGNED_NOTIFICATION";

        /// <summary>
        /// The message sent to the user when the operator edits the ticket or sent to the operator when the user edits the ticket.
        /// </summary>
        public const string TicketEdited = "TICKET_EDITED_NOTIFICATION";

        /// <summary>
        /// The message sent to the operator when a ticket is assigned to them.
        /// </summary>
        public const string TicketAssignedToYou = "TICKET_ASSIGNED_TO_YOU_NOTIFICATION";
    }
}
