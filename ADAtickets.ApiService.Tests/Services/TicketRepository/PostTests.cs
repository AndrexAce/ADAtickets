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
using ADAtickets.ApiService.Configs;
using ADAtickets.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using TicketService = ADAtickets.ApiService.Services.TicketRepository;

namespace ADAtickets.ApiService.Tests.Services.TicketRepository
{
    /// <summary>
    /// <c>AddTicketAsync(Ticket)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PostTests
    {
        public static TheoryData<Ticket> InvalidTicketData =>
        [
            Utilities.CreateTicket(title: new string('a', 51), description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: null),
            Utilities.CreateTicket(title: new string('a', 51), description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: Guid.Empty),
            Utilities.CreateTicket(title: "Title.", description: new string('a', 5001), platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: null),
            Utilities.CreateTicket(title: "Title.", description: new string('a', 5001), platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: Guid.Empty),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.Empty, creatorUserId: Guid.AllBitsSet, operatorUserId: null),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.Empty, creatorUserId: Guid.AllBitsSet, operatorUserId: Guid.Empty),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.NewGuid(), operatorUserId: null),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.NewGuid(), operatorUserId: Guid.Empty),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: Guid.NewGuid()),
        ];

        public static TheoryData<Ticket> ValidTicketData =>
        [
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: null),
            Utilities.CreateTicket(title: "Title.", description: "Description.", platformId: Guid.AllBitsSet, creatorUserId: Guid.AllBitsSet, operatorUserId: Guid.Empty)
        ];

        [Theory]
        [MemberData(nameof(ValidTicketData))]
        public async Task AddTicket_ValidEntity_ReturnsTicket(Ticket inTicket)
        {
            // Arrange
            var tickets = new List<Ticket>();
            var platform = new List<Platform> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockPlatformSet = platform.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockTicketSet.Setup(s => s.Add(It.IsAny<Ticket>()))
                .Callback<Ticket>(t =>
                {
                    if (t.Title.Length <= 50 && t.Description.Length <= 5000
                    && mockPlatformSet.Object.Single().Id == t.PlatformId
                    && mockUserSet.Object.ElementAt(0).Id == t.CreatorUserId
                    && (t.OperatorUserId == null || mockUserSet.Object.ElementAt(1).Id == t.OperatorUserId))
                    {
                        tickets.Add(t);
                    }
                });
            mockContext.Setup(c => c.Tickets)
                .Returns(mockTicketSet.Object);

            var service = new TicketService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddTicketAsync(inTicket);
            var addedTicket = await mockContext.Object.Tickets.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(addedTicket);
            Assert.NotEmpty(tickets);
        }

        [Theory]
        [MemberData(nameof(InvalidTicketData))]
        public async Task AddTicket_InvalidEntity_ReturnsNothing(Ticket inTicket)
        {
            // Arrange
            var tickets = new List<Ticket>();
            var platform = new List<Platform> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockPlatformSet = platform.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockTicketSet.Setup(s => s.Add(It.IsAny<Ticket>()))
                .Callback<Ticket>(t =>
                {
                    if (t.Title.Length <= 50 && t.Description.Length <= 5000
                    && mockPlatformSet.Object.Single().Id == t.PlatformId
                    && mockUserSet.Object.ElementAt(0).Id == t.CreatorUserId
                    && (t.OperatorUserId == null || mockUserSet.Object.ElementAt(1).Id == t.OperatorUserId))
                    {
                        tickets.Add(t);
                    }
                });
            mockContext.Setup(c => c.Tickets)
                .Returns(mockTicketSet.Object);

            var service = new TicketService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddTicketAsync(inTicket);
            var addedTicket = await mockContext.Object.Tickets.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(addedTicket);
            Assert.Empty(tickets);
        }
    }
}
