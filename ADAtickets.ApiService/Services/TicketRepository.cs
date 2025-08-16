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

using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ADAtickets.ApiService.Services;

/// <summary>
///     Implements the methods to manage the <see cref="Ticket" /> entities in the underlying database.
/// </summary>
internal class TicketRepository(ADAticketsDbContext context) : ITicketRepository
{
    /// <inheritdoc cref="ITicketRepository.GetTicketByIdAsync" />
    public async Task<Ticket?> GetTicketByIdAsync(Guid id)
    {
        return await context.Tickets.Include(t => t.CreatorUser)
            .Include(t => t.OperatorUser)
            .Include(t => t.Platform)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc cref="ITicketRepository.GetTicketsAsync" />
    public async Task<IEnumerable<Ticket>> GetTicketsAsync()
    {
        return await context.Tickets.Include(t => t.CreatorUser)
            .Include(t => t.OperatorUser)
            .Include(t => t.Platform)
            .ToListAsync();
    }

    /// <inheritdoc cref="ITicketRepository.GetTicketsByAsync" />
    [SuppressMessage("Performance",
        "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
        Justification =
            "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
    public async Task<IEnumerable<Ticket>> GetTicketsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
    {
        IQueryable<Ticket> query = context.Tickets.Include(t => t.CreatorUser)
            .Include(t => t.OperatorUser)
            .Include(t => t.Platform);

        foreach (var filter in filters)
            switch (filter.Key.Pascalize())
            {
                case nameof(Ticket.Id) when Guid.TryParse(filter.Value, out var outId):
                    query = query.Where(ticket => ticket.Id == outId);
                    break;

                case nameof(Ticket.Type) when Enum.TryParse(filter.Value, true, out TicketType outType):
                    query = query.Where(ticket => ticket.Type == outType);
                    break;

                case nameof(Ticket.CreationDateTime) when DateTimeOffset.TryParse(filter.Value,
                    CultureInfo.InvariantCulture, out var outCreationDateTime):
                    query = query.Where(ticket => ticket.CreationDateTime.Date == outCreationDateTime.Date);
                    break;

                case nameof(Ticket.Title):
                    query = query.Where(ticket => ticket.Title.ToLower().Contains(filter.Value.ToLower()));
                    break;

                case nameof(Ticket.Description):
                    query = query.Where(ticket => ticket.Description.ToLower().Contains(filter.Value.ToLower()));
                    break;

                case nameof(Ticket.Priority) when Enum.TryParse(filter.Value, true, out Priority outPriority):
                    query = query.Where(ticket => ticket.Priority == outPriority);
                    break;

                case nameof(Ticket.Status) when Enum.TryParse(filter.Value, true, out Status outStatus):
                    query = query.Where(ticket => ticket.Status == outStatus);
                    break;

                case nameof(Ticket.WorkItemId) when int.TryParse(filter.Value, out var outWorkItemId):
                    query = query.Where(ticket => ticket.WorkItemId == outWorkItemId);
                    break;

                case nameof(Ticket.PlatformId) when Guid.TryParse(filter.Value, out var outPlatformId):
                    query = query.Where(ticket => ticket.PlatformId == outPlatformId);
                    break;

                case nameof(Ticket.CreatorUserId) when Guid.TryParse(filter.Value, out var outCreatorUserId):
                    query = query.Where(ticket => ticket.CreatorUserId == outCreatorUserId);
                    break;

                case nameof(Ticket.OperatorUserId) when Guid.TryParse(filter.Value, out var outOperatorUserId):
                    query = query.Where(ticket => ticket.OperatorUserId == outOperatorUserId);
                    break;
            }

        return await query.ToListAsync();
    }

    /// <inheritdoc cref="ITicketRepository.AddTicketAsync" />
    public async Task AddTicketAsync(Ticket ticket)
    {
        _ = context.Tickets.Add(ticket);
        _ = await context.SaveChangesAsync();
    }

    /// <inheritdoc cref="ITicketRepository.UpdateTicketAsync" />
    public async Task UpdateTicketAsync(Ticket ticket)
    {
        _ = context.Tickets.Update(ticket);
        _ = await context.SaveChangesAsync();
    }

    /// <inheritdoc cref="ITicketRepository.DeleteTicketAsync" />
    public async Task DeleteTicketAsync(Ticket ticket)
    {
        _ = context.Tickets.Remove(ticket);
        _ = await context.SaveChangesAsync();
    }
}