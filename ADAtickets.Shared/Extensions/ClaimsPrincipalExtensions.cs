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
using System.Security.Claims;

namespace ADAtickets.Shared.Extensions
{
    /// <summary>
    /// Exposes extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the email address associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns>Email of the identity, or <see langword="null"/> if it cannot be found.</returns>
        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst("preferred_username")?.Value;
        }

        /// <summary>
        /// Gets the display name associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns>Display name of the identity, or <see langword="null"/> if it cannot be found.</returns>
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst("name")?.Value;
        }

        /// <summary>
        /// Gets the full name associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns>Full name of the identity, or <see langword="null"/> if it cannot be found.</returns>
        public static string? GetFullName(this ClaimsPrincipal user)
        {
            string? name = user.GetName();
            string? surname = user.GetSurname();

            return string.IsNullOrEmpty(name) && string.IsNullOrEmpty(surname) ? null : $"{name} {surname}";
        }

        /// <summary>
        /// Gets the name associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns>Name of the identity, or <see langword="null"/> if it cannot be found.</returns>
        public static string? GetName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.GivenName)?.Value;
        }

        /// <summary>
        /// Gets the surname associated with the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns>Surname of the identity, or <see langword="null"/> if it cannot be found.</returns>
        public static string? GetSurname(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Surname)?.Value;
        }

        /// <summary>
        /// Checks if the <see cref="ClaimsPrincipal"/> is a DevOps administrator.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> of which the claim must be checked.</param>
        /// <returns><see langword="true"/> if the <see cref="ClaimsPrincipal"/> is a DevOps administrator; otherwise, <see langword="false"/>.</returns>
        public static bool IsDevOpsAdmin(this ClaimsPrincipal user)
        {
            return user.FindFirst("wids")?.Value == "e3973bdf-4987-49ae-837a-ba8e231c7286";
        }

        /// <summary>
        /// Checks if the <see cref="ClaimsPrincipal"/> is authenticated.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> from which to retrieve the claim.</param>
        /// <returns><see langword="true"/> if the <see cref="ClaimsPrincipal"/> is authenticated; otherwise, <see langword="false"/>.</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated ?? false;
        }
    }
}