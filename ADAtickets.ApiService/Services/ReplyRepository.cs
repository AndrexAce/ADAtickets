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
    /// Implements the methods to manage the <see cref="Reply"/> entities in the underlying database.
    /// </summary>
    class ReplyRepository(ADAticketsDbContext context) : IReplyRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IReplyRepository.GetReplyByIdAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity was not found.</exception>
        public async Task<Reply> GetReplyByIdAsync(Guid id)
        {
            return await _context.Replies.FindAsync(id) ?? throw new InvalidOperationException($"Entity of type {typeof(Reply)} with id {id} was not found.");
        }

        /// <inheritdoc cref="IReplyRepository.GetRepliesAsync"/>
        public async IAsyncEnumerable<Reply> GetRepliesAsync()
        {
            await foreach (var reply in _context.Replies.AsAsyncEnumerable())
            {
                yield return reply;
            }
        }

        /// <inheritdoc cref="IReplyRepository.AddReplyAsync(Reply)"/>
        /// <exception cref="DbUpdateException">When the entity was not added because of a conflict.</exception>
        public async Task AddReplyAsync(Reply reply)
        {
            await _context.Replies.AddAsync(reply);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IReplyRepository.UpdateReplyAsync(Reply)"/>
        /// <exception cref="DbUpdateException">When the entity was not updated because of a conflict.</exception>
        public async Task UpdateReplyAsync(Reply reply)
        {
            _context.Replies.Update(reply);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IReplyRepository.DeleteReplyAsync(Guid)"/>
        /// <exception cref="InvalidOperationException">When the entity to delete was not found.</exception>
        public async Task DeleteReplyAsync(Guid id)
        {
            if (await _context.Replies.FindAsync(id) is not Reply reply)
                throw new InvalidOperationException($"Entity of type {typeof(Reply)} with id {id} was not found.");
            _context.Remove(reply);
            await _context.SaveChangesAsync();
        }
    }
}
