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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.Shared.Models;

/// <summary>
///     Represents a platform managed by the enterprise which tickets are related to.
/// </summary>
[Index(nameof(RepositoryUrl), IsUnique = true)]
public sealed class Platform : Entity
{
    /// <summary>
    ///     The name of the platform.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Unicode]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The URL of the repository where the source code of the platform is hosted.
    /// </summary>
    [Required]
    [MaxLength(4000)]
    [Unicode(false)]
    [StringSyntax(StringSyntaxAttribute.Uri)]
    [RegularExpression(@"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}\/?$")]
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    ///     The collection of the tickets related to the platform.
    /// </summary>
    [Required]
    [InverseProperty(nameof(Ticket.Platform))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<Ticket> Tickets { get; } = [];

    /// <summary>
    ///     Join-entity between the platform and the users who marked it as preferred.
    /// </summary>
    [Required]
    [InverseProperty(nameof(UserPlatform.Platform))]
    [AdaptIgnore]
    [JsonIgnore]
    public ICollection<UserPlatform> UserPlatforms { get; } = [];
}