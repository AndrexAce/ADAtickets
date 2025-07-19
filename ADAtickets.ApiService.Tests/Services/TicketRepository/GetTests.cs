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
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetTicketByIdAsync_ExistingId_ReturnsTicket()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<Ticket> tickets = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            Ticket? result = await service.GetTicketByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetTicketByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<Ticket> tickets = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            Ticket? result = await service.GetTicketByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTicketByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<Ticket> tickets = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => tickets.Find(t => t.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            Ticket? result = await service.GetTicketByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetTickets_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<Ticket> tickets = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTickets_FullSet_ReturnsTickets()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<Ticket> tickets =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetTicketsBy_OneFilterWithMatch_ReturnsTickets()
        {
            // Arrange
            List<Ticket> tickets =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsByAsync([new KeyValuePair<string, string>("Description", "description")]);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains("description", result.ElementAt(0).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(1).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(2).Description, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetTicketsBy_MoreFiltersWithMatch_ReturnTickets()
        {
            // Arrange
            List<Ticket> tickets =
            [
                new() { Description = "Example description.", Status = Status.Closed },
                new() { Description = "Trial description." },
                new() { Description = "Test description.", Status = Status.Closed }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsByAsync([
                new KeyValuePair<string, string>("Description", "description"),
                new KeyValuePair<string, string>("Status", Status.Closed.ToString())
                ]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("description", result.ElementAt(0).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(1).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Equal(Status.Closed, result.ElementAt(0).Status);
            Assert.Equal(Status.Closed, result.ElementAt(1).Status);
        }

        [Fact]
        public async Task GetTicketsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<Ticket> tickets =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsByAsync([new KeyValuePair<string, string>("Description", "text")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsNothing()
        {
            // Arrange
            List<Ticket> tickets =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Ticket>> mockSet = tickets.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            TicketService service = new(mockContext.Object);

            // Act
            IEnumerable<Ticket> result = await service.GetTicketsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Empty(result);
        }
        #endregion
    }
}