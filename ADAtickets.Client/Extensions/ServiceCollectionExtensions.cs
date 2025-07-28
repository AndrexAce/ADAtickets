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
using Microsoft.Extensions.DependencyInjection;

namespace ADAtickets.Client.Extensions
{
    /// <summary>
    /// Exposes extension methods for <see cref="IServiceCollection"/> to register ADAtickets client services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the ADAtickets client objects to interact with ADAtickets APIs programmatically.
        /// </summary>
        public static IServiceCollection AddADAticketsClient(this IServiceCollection services)
        {
            _ = services.AddScoped<AzureDevOpsClient>();
            _ = services.AddScoped<AttachmentsClient>();
            _ = services.AddScoped<EditsClient>();
            _ = services.AddScoped<NotificationsClient>();
            _ = services.AddScoped<PlatformsClient>();
            _ = services.AddScoped<RepliesClient>();
            _ = services.AddScoped<TicketsClient>();
            _ = services.AddScoped<UsersClient>();
            _ = services.AddScoped<UserPlatformsClient>();

            return services;
        }
    }
}