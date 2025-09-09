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
using Newtonsoft.Json;

namespace ADAtickets.Shared.Dtos.Responses;

/// <summary>
///     Represents a paginated collection of response DTOs.
/// </summary>
/// <typeparam name="TResponse">The type of response DTO contained in the page, which must inherit from <see cref="ResponseDto"/>.</typeparam>
public sealed record Page<TResponse> where TResponse : ResponseDto
{
    /// <summary>
    ///     The current page number (1-based).
    /// </summary>
    [JsonProperty]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    ///     The number of items in the current page.
    /// </summary>
    [JsonProperty]
    public int PageSize { get; init; } = 0;

    /// <summary>
    ///     The next page number, or <see langword="null"/> if this is the last page.
    /// </summary>
    [JsonProperty]
    public int? NextPage { get; init; } = null;

    /// <summary>
    ///     The previous page number, or <see langword="null"/> if this is the first page.
    /// </summary>
    [JsonProperty]
    public int? PreviousPage { get; init; } = null;

    /// <summary>
    ///     The total number of pages available.
    /// </summary>
    [JsonProperty]
    public int TotalPages { get; init; } = 1;

    /// <summary>
    ///     The total number of items across all pages.
    /// </summary>
    [JsonProperty]
    public int TotalItems { get; init; } = 0;

    /// <summary>
    ///     The collection of items in the current page.
    /// </summary>
    [JsonProperty]
    public IEnumerable<TResponse> Items { get; init; } = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="Page{T}"/> record with default values.
    /// </summary>
    public Page() { }

    /// <summary>
    ///     Creates a new page of entities which contains all the entities, unpaged.
    /// </summary>
    /// <param name="entities">The collection of entities to include in the page.</param>
    internal Page(IEnumerable<TResponse> entities)
    {
        var entitiesCount = entities.Count();

        PageSize = entitiesCount;
        TotalItems = entitiesCount;
        Items = entities;
    }

    /// <summary>
    ///     Creates a new page of entities with the given <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
    /// </summary>
    /// <param name="entities">Enumerable of <typeparamref name="T"/> to page.</param>
    /// <param name="pageNumber">The page number (starts from 1).</param>
    /// <param name="pageSize">The number of entities to fetch in the page.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than or equal to zero,
    ///     or when <paramref name="pageNumber"/> exceeds the total number of pages.
    /// </exception>
    /// <remarks>Both <paramref name="pageNumber"/> and <paramref name="pageSize"/> must be greater than 0.</remarks>
    internal Page(IEnumerable<TResponse> entities, int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var entitiesCount = entities.Count();

        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = entitiesCount;

        TotalPages = (int)Math.Ceiling((double)entitiesCount / pageSize);
        PreviousPage = pageNumber > 1 ? pageNumber - 1 : null;
        NextPage = pageNumber < TotalPages ? pageNumber + 1 : null;

        if (pageNumber > TotalPages && TotalPages > 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        Items = entities.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
    }
}
