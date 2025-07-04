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
    /// Represents the services used by the applications.
    /// </summary>
    static class Service
    {
        /// <summary>
        /// The name of the database service.
        /// </summary>
        public const string Database = "PostgreSQL";
        /// <summary>
        /// The name of the cache service.
        /// </summary>
        public const string Cache = "Redis";
        /// <summary>
        /// The name of the API service used by internal users.
        /// </summary>
        public const string API = "ADAticketsAPI";
        /// <summary>
        /// The name of the API service used by external users.
        /// </summary>
        public const string ExternalAPI = "ExternalADAticketsAPI";
        /// <summary>
        /// The name of the Microsoft Graph service used by internal users.
        /// </summary>
        public const string Graph = "Graph";
        /// <summary>
        /// The name of the Microsoft Graph service used by external users.
        /// </summary>
        public const string ExternalGraph = "ExternalGraph";

        /// <summary>
        /// The version of the database service.
        /// </summary>
        public const string DatabaseVersion = "17.5";
        /// <summary>
        /// The name of the cache service.
        /// </summary>
        public const string CacheVersion = "8.0.2";
        /// <summary>
        /// The version of the API service.
        /// </summary>
        public const string APIVersion = "1";
        /// <summary>
        /// The version of the Azure DevOps API service used to interact with Azure DevOps repositories.
        /// </summary>
        public const string AzureDevOpsAPIVersion = "7.2-preview";
    }
}
