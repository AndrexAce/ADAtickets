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
using ADAtickets.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using ReplyService = ADAtickets.ApiService.Services.ReplyRepository;

namespace ADAtickets.ApiService.Tests.Services.ReplyRepository
{
    /// <summary>
    /// <c>UpdateReply(Reply)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PutTests
    {
        public static TheoryData<Reply> InvalidReplyData =>
        [
            Utilities.CreateReply(message: new string('a', 5001), authorUserId: Guid.AllBitsSet, ticketId: Guid.AllBitsSet),
            Utilities.CreateReply(message: "Valid message.", authorUserId: Guid.Empty, ticketId: Guid.AllBitsSet),
            Utilities.CreateReply(message: "Valid message.", authorUserId: Guid.AllBitsSet, ticketId: Guid.Empty)
        ];

        public static TheoryData<Reply> ValidReplyData =>
        [
            Utilities.CreateReply(message: "Valid message.", authorUserId: Guid.AllBitsSet, ticketId: Guid.AllBitsSet)
        ];

        [Theory]
        [MemberData(nameof(ValidReplyData))]
        public async Task UpdateReply_ValidEntity_ReturnsNew(Reply inReply)
        {
            // Arrange
            var replies = new List<Reply> { new() { Id = inReply.Id, Message = "Old message.", AuthorUserId = Guid.AllBitsSet, TicketId = Guid.AllBitsSet } };
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockReplySet = replies.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockReplySet.Setup(s => s.Update(It.IsAny<Reply>()))
                .Callback<Reply>(r =>
                {
                    if (r.Message.Length <= 5000 && mockTicketSet.Object.Single().Id == r.TicketId && mockUserSet.Object.Single().Id == r.AuthorUserId)
                    {
                        replies[0].Message = inReply.Message;
                    }
                });
            mockContext.Setup(c => c.Replies)
                .Returns(mockReplySet.Object);

            var service = new ReplyService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateReplyAsync(inReply);
            var updatedReply = await mockContext.Object.Replies.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedReply);
            Assert.Equal(inReply.Message, updatedReply.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidReplyData))]
        public async Task UpdateReply_InvalidEntity_ReturnsOld(Reply inReply)
        {
            // Arrange
            var replies = new List<Reply> { new() { Id = inReply.Id, Message = "Old message.", AuthorUserId = Guid.AllBitsSet, TicketId = Guid.AllBitsSet } };
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockReplySet = replies.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockReplySet.Setup(s => s.Update(It.IsAny<Reply>()))
                .Callback<Reply>(r =>
                {
                    if (r.Message.Length <= 5000 && mockTicketSet.Object.Single().Id == r.TicketId && mockUserSet.Object.Single().Id == r.AuthorUserId)
                    {
                        replies[0].Message = inReply.Message;
                    }
                });
            mockContext.Setup(c => c.Replies)
                .Returns(mockReplySet.Object);

            var service = new ReplyService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateReplyAsync(inReply);
            var updatedReply = await mockContext.Object.Replies.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedReply);
            Assert.NotEqual(inReply.Message, updatedReply.Message);
        }
    }
}
