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
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// <para>Represents the Entity Framework Core database context for the ADAtickets application.</para>
    /// <para>This context is used to interact with the database and manage the application's entities.</para>
    /// </summary>
    sealed class ADAticketsDbContext(DbContextOptions<ADAticketsDbContext> options) : DbContext(options)
    {
        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Attachment"/> entities.
        /// </value>
        internal DbSet<Attachment> Attachments { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Edit"/> entities.
        /// </value>
        internal DbSet<Edit> Edits { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Notification"/> entities.
        /// </value>
        internal DbSet<Notification> Notifications { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Platform"/> entities.
        /// </value>
        internal DbSet<Platform> Platforms { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Reply"/> entities.
        /// </value>
        internal DbSet<Reply> Replies { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="Ticket"/> entities.
        /// </value>
        internal DbSet<Ticket> Tickets { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="User"/> entities.
        /// </value>
        internal DbSet<User> Users { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="UserNotification"/> entities.
        /// </value>
        internal DbSet<UserNotification> UserNotifications { get; set; }

        /// <value>
        /// Gets or sets the DbSet for managing <see cref="UserPlatform"/> entities.
        /// </value>
        internal DbSet<UserPlatform> UserPlatforms { get; set; }
    }
}
