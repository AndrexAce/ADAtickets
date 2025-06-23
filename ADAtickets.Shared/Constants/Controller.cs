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

namespace ADAtickets.Shared.Constants
{
    /// <summary>
    /// Represents the path of an API controller.
    /// </summary>
    static class Controller
    {
        /// <summary>
        /// Path to the API controller for <see cref="Attachment"/> entities.
        /// </summary>
        public const string Attachments = "Attachments";
        /// <summary>
        /// Path to the API controller for Azure DevOps operations.
        /// </summary>
        public const string AzureDevOps = "AzureDevOps";
        /// <summary>
        /// Path to the API controller for <see cref="Edit"/> entities.
        /// </summary>
        public const string Edits = "Edits";
        /// <summary>
        /// Path to the API controller for <see cref="Notification"/> entities.
        /// </summary>
        public const string Notifications = "Notifications";
        /// <summary>
        /// Path to the API controller for <see cref="Platform"/> entities.
        /// </summary>
        public const string Platforms = "Platforms";
        /// <summary>
        /// Path to the API controller for <see cref="Reply"/> entities.
        /// </summary>
        public const string Replies = "Replies";
        /// <summary>
        /// Path to the API controller for <see cref="Ticket"/> entities.
        /// </summary>
        public const string Tickets = "Tickets";
        /// <summary>
        /// Path to the API controller for <see cref="User"/> entities.
        /// </summary>
        public const string Users = "Users";
    }
}
