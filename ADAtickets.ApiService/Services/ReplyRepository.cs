﻿/*
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
    /// Implements the methods to manage the <see cref="Reply"/> entities in the underlying database.
    /// </summary>
    class ReplyRepository(ADAticketsDbContext context) : IReplyRepository
    {
        /// <inheritdoc cref="IReplyRepository.GetReplyByIdAsync"/>
        public async Task<Reply?> GetReplyByIdAsync(Guid id)
        {
            return await context.Replies.FindAsync(id);
        }

        /// <inheritdoc cref="IReplyRepository.GetRepliesAsync"/>
        public async Task<IEnumerable<Reply>> GetRepliesAsync()
        {
            return await context.Replies.ToListAsync();
        }

        /// <inheritdoc cref="IReplyRepository.GetRepliesByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<Reply>> GetRepliesByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Reply> query = context.Replies;

            foreach (var filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Platform.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(platform => platform.Id == outGuid);
                        break;

                    case nameof(Reply.ReplyDateTime) when DateTimeOffset.TryParse(filter.Value, CultureInfo.InvariantCulture, out DateTimeOffset outDateTimeOffset):
                        query = query.Where(reply => reply.ReplyDateTime.Date == outDateTimeOffset.Date);
                        break;

                    case nameof(Reply.Message):
                        query = query.Where(reply => reply.Message.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    case nameof(Reply.AuthorUserId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(reply => reply.AuthorUserId == outGuid);
                        break;

                    case nameof(Reply.TicketId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(reply => reply.TicketId == outGuid);
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IReplyRepository.AddReplyAsync"/>
        public async Task AddReplyAsync(Reply reply)
        {
            context.Replies.Add(reply);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IReplyRepository.UpdateReplyAsync"/>
        public async Task UpdateReplyAsync(Reply reply)
        {
            context.Replies.Update(reply);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc cref="IReplyRepository.DeleteReplyAsync"/>
        public async Task DeleteReplyAsync(Reply reply)
        {
            context.Replies.Remove(reply);
            await context.SaveChangesAsync();
        }
    }
}
