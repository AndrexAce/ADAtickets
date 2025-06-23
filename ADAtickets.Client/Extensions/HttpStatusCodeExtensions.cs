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
using System.Net;

namespace ADAtickets.Client.Extensions
{
    /// <summary>
    /// Exposes extension methods for <see cref="HttpStatusCode"/>.
    /// </summary>
    public static class HttpStatusCodeExtensions
    {
        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is an informational status code (1xx).
        /// </summary>
        /// <param name="statusCode">Status code to check for.</param>
        /// <returns><see langword="true"/> if the status code is informational; otherwise, <see langword="false"/>.</returns>
        public static bool IsInformation(this HttpStatusCode statusCode) => ((int)statusCode >= 100) && ((int)statusCode <= 199);

        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is a success status code (2xx).
        /// </summary>
        /// <param name="statusCode">Status code to check for.</param>
        /// <returns><see langword="true"/> if the status code is successful; otherwise, <see langword="false"/>.</returns>
        public static bool IsSuccess(this HttpStatusCode statusCode) => ((int)statusCode >= 200) && ((int)statusCode <= 299);

        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is a redirectional status code (3xx).
        /// </summary>
        /// <param name="statusCode">Status code to check for.</param>
        /// <returns><see langword="true"/> if the status code is redirectional; otherwise, <see langword="false"/>.</returns>
        public static bool IsRedirection(this HttpStatusCode statusCode) => ((int)statusCode >= 300) && ((int)statusCode <= 399);

        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is a client error status code (4xx).
        /// </summary>
        /// <param name="statusCode">Status code to check for.</param>
        /// <returns><see langword="true"/> if the status code is a client error; otherwise, <see langword="false"/>.</returns>
        public static bool IsClientError(this HttpStatusCode statusCode) => ((int)statusCode >= 400) && ((int)statusCode <= 499);

        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is a server error status code (5xx).
        /// </summary>
        /// <param name="statusCode">Status code to check for.</param>
        /// <returns><see langword="true"/> if the status code is a server error; otherwise, <see langword="false"/>.</returns>
        public static bool IsServerError(this HttpStatusCode statusCode) => ((int)statusCode >= 500) && ((int)statusCode <= 599);

        /// <summary>
        /// Checks if the <see cref="HttpStatusCode"/> is an error status code (4xx or 5xx).
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns><see langword="true"/> if the status code is an error; otherwise, <see langword="false"/>.</returns>
        public static bool IsError(this HttpStatusCode statusCode) => statusCode.IsClientError() || statusCode.IsServerError();
    }
}