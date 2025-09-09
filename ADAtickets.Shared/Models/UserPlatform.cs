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

using AutoMapper.Configuration.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents the link between users and their preferred platforms.
/// </summary>
/// <remarks>This class is intended for internal use only; it is public only to allow for testing.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[Index(nameof(UserId), nameof(PlatformId), IsUnique = true)]
public sealed class UserPlatform : Entity
{
    /// <summary>
    ///     The unique identifier of the user who marked the platform as preferred.
    /// </summary>
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The user who marked the platform as preferred.
    /// </summary>
    [Required]
    [Ignore]
    [JsonIgnore]
    public User User { get; set; } = null!;

    /// <summary>
    ///     The unique identifier of the platform that was marked as preferred.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public Guid PlatformId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The platform that was marked as preferred.
    /// </summary>
    [Required]
    [Ignore]
    [JsonIgnore]
    public Platform Platform { get; set; } = null!;
}