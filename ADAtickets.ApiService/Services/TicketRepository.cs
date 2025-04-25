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
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Ticket"/> entities in the underlying database.
    /// </summary>
    class TicketRepository(ADAticketsDbContext context) : ITicketRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="ITicketRepository.GetTicketByIdAsync(Guid)"/>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public async Task<Ticket> GetTicketByIdAsync(Guid id)
        {
            return await _context.Tickets.FindAsync(id) ?? throw new InvalidOperationException($"Entity of type {typeof(Ticket)} with id {id} was not found.");
        }

        /// <inheritdoc cref="ITicketRepository.GetTicketsAsync"/>
        public async IAsyncEnumerable<Ticket> GetTicketsAsync()
        {
            await foreach (var ticket in _context.Tickets.AsAsyncEnumerable())
            {
                yield return ticket;
            }
        }

        /// <inheritdoc cref="ITicketRepository.GetTicketsByUserIdAsync(Guid)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddTicketAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="ITicketRepository.UpdateTicketAsync(Ticket)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflict.</exception>
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="ITicketRepository.DeleteTicketAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>
        public async Task DeleteTicketAsync(Guid id)
        {
            if (await _context.Tickets.FindAsync(id) is not Ticket ticket)
                throw new InvalidOperationException($"Entity of type {typeof(Ticket)} with id {id} was not found.");
            _context.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
