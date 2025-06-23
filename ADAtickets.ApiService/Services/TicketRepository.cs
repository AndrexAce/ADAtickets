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
using System.Globalization;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Ticket"/> entities in the underlying database.
    /// </summary>
    class TicketRepository(ADAticketsDbContext context) : ITicketRepository
    {
        /// <inheritdoc cref="ITicketRepository.GetTicketByIdAsync"/>
        public async Task<Ticket?> GetTicketByIdAsync(Guid id)
        {
            return await context.Tickets.FindAsync(id);
        }

        /// <inheritdoc cref="ITicketRepository.GetTicketsAsync"/>
        public async Task<IEnumerable<Ticket>> GetTicketsAsync()
        {
            return await context.Tickets.ToListAsync();
        }

        /// <inheritdoc cref="ITicketRepository.GetTicketsByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<Ticket>> GetTicketsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Ticket> query = context.Tickets;

            foreach (var filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Ticket.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(ticket => ticket.Id == outGuid);
                        break;

                    case nameof(Ticket.Type) when Enum.TryParse(filter.Value, true, out TicketType outTicketType):
                        query = query.Where(ticket => ticket.Type == outTicketType);
                        break;

                    case nameof(Ticket.CreationDateTime) when DateTimeOffset.TryParse(filter.Value, CultureInfo.InvariantCulture, out DateTimeOffset outDateTimeOffset):
                        query = query.Where(ticket => ticket.CreationDateTime.Date == outDateTimeOffset.Date);
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

                    case nameof(Ticket.WorkItemId) when int.TryParse(filter.Value, out int outInt):
                        query = query.Where(ticket => ticket.WorkItemId == outInt);
                        break;

                    case nameof(Ticket.PlatformId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(ticket => ticket.PlatformId == outGuid);
                        break;

                    case nameof(Ticket.CreatorUserId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(ticket => ticket.CreatorUserId == outGuid);
                        break;

                    case nameof(Ticket.OperatorUserId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(ticket => ticket.OperatorUserId == outGuid);
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="ITicketRepository.AddTicketAsync"/>
        public async Task AddTicketAsync(Ticket ticket)
        {
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="ITicketRepository.UpdateTicketAsync"/>
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="ITicketRepository.DeleteTicketAsync"/>
        public async Task DeleteTicketAsync(Ticket ticket)
        {
            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
        }
    }
}
