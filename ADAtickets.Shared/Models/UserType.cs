﻿/*
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
namespace ADAtickets.Shared.Models
{
    /// <summary>
    /// Represents the role of the user in the system.
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// The user can create and reply to their tickets.
        /// </summary>
        User,
        /// <summary>
        /// The operator can manage all the tickets.
        /// </summary>
        Operator,
        /// <summary>
        /// The administrator can manage all the tickets, platforms and users.
        /// </summary>
        Admin
    }
}
