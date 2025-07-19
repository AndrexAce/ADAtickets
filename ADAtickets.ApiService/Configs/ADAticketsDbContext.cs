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
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Configs
{
    /// <summary>
    /// <para>Represents the Entity Framework Core database context for the ADAtickets application.</para>
    /// <para>This context is used to interact with the database and manage the application entities.</para>
    /// </summary>
    internal class ADAticketsDbContext : DbContext
    {
        /// <summary>
        /// Paramteless constructor for testing purposes.
        /// </summary>
        public ADAticketsDbContext() { }

        /// <summary>
        /// Costructor to use dependency injection with ASP.NET.
        /// </summary>
        /// <param name="options">The database configuration options.</param>
        public ADAticketsDbContext(DbContextOptions<ADAticketsDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Attachment"/> entities.
        /// </summary>
        public virtual DbSet<Attachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Edit"/> entities.
        /// </summary>
        public virtual DbSet<Edit> Edits { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Notification"/> entities.
        /// </summary>
        public virtual DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Platform"/> entities.
        /// </summary>
        public virtual DbSet<Platform> Platforms { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Reply"/> entities.
        /// </summary>
        public virtual DbSet<Reply> Replies { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="Ticket"/> entities.
        /// </summary>
        public virtual DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="User"/> entities.
        /// </summary>
        public virtual DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="UserNotification"/> entities.
        /// </summary>
        public virtual DbSet<UserNotification> UserNotifications { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for managing <see cref="UserPlatform"/> entities.
        /// </summary>
        public virtual DbSet<UserPlatform> UserPlatforms { get; set; }
    }
}
