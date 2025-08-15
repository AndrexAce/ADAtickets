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
using ADAtickets.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using ReplyService = ADAtickets.ApiService.Services.ReplyRepository;

namespace ADAtickets.Tests.Services.ReplyRepository
{
    /// <summary>
    /// <c>DeleteReplyByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    public sealed class DeleteTests
    {
        [Fact]
        public async Task DeleteReplyByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            Reply reply = new() { Id = Guid.NewGuid() };
            List<Reply> replies = [reply];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Reply>> mockSet = replies.BuildMockDbSet();
            _ = mockSet.Setup(s => s.Remove(It.IsAny<Reply>()))
                .Callback<Reply>(reply => replies.RemoveAll(r => r.Id == reply.Id));
            _ = mockContext.Setup(c => c.Replies)
                .Returns(mockSet.Object);

            ReplyService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteReplyAsync(reply);
            Reply? deletedReply = await mockContext.Object.Replies.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedReply);
        }
    }
}