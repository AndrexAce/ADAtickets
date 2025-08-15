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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutoMapper.Configuration.Annotations;

namespace ADAtickets.Shared.Models;

/// <summary>
///     <para>Base class for all entities in the ADAtickets model.</para>
///     <para>Contains common properties and methods.</para>
/// </summary>
/// <remarks>This class is intended for internal use only; it is public only to allow for testing.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class Entity
{
    /// <summary>
    ///     The unique identifier of the entity.
    /// </summary>
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     Reserved field used to detect concurrent modification to the entity.
    /// </summary>
    [Timestamp]
    [Ignore]
    [JsonIgnore]
    public uint Version { get; set; } = 0;
}