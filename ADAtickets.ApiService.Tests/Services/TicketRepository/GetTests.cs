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
using ADAtickets.ApiService.Models;
using MockQueryable.Moq;
using Moq;
using TicketService = ADAtickets.ApiService.Services.TicketRepository;

namespace ADAtickets.ApiService.Tests.Services.TicketRepository
{
    /// <summary>
    /// <c>GetTicketByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetTicketsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    sealed public class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetTicketByIdAsync_ExistingId_ReturnsTicket()
        {
            // Arrange
            var existingId = Guid.NewGuid();

            var tickets = new List<Ticket> { new() { Id = existingId } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            // Act
            var result = await service.GetTicketByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetTicketByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var tickets = new List<Ticket> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            // Act
            var result = await service.GetTicketByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTicketByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            var tickets = new List<Ticket> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            // Act
            var result = await service.GetTicketByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetTickets_EmptySet_ReturnsNothing()
        {
            // Arrange
            var tickets = new List<Ticket>();

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            // Act
            var result = await service.GetTicketsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTickets_FullSet_ReturnsTickets()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            var tickets = new List<Ticket> {
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            // Act
            var result = await service.GetTicketsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion
    }
}