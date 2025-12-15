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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.Api;

internal sealed class AdaTicketsDbContext(DbContextOptions<AdaTicketsDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Attachment> Attachments { get; set; }

    public DbSet<Edit> Edits { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Platform> Platforms { get; set; }

    public DbSet<Reply> Replies { get; set; }

    public DbSet<Ticket> Tickets { get; set; }

    public DbSet<AppUser> AppUsers { get; set; }

    public DbSet<UserNotification> UserNotifications { get; set; }

    public DbSet<UserPlatform> UserPlatforms { get; set; }
}