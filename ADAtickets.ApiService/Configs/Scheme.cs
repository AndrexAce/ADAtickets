/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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
namespace ADAtickets.ApiService.Configs
{
    /// <summary>
    /// Represents the scheme for the authorization.
    /// </summary>
    enum Scheme
    {
        /// <summary>
        /// Scheme employed by logged in admins in the organizational tenant.
        /// </summary>
        Entra,
        /// <summary>
        /// Scheme employed by the logged in user or opertor in the external tenant.
        /// </summary>
        ExternalEntra,
        /// <summary>
        /// Scheme employed by the service principal in the organizational tenant.
        /// </summary>
        DevOps
    }
}
