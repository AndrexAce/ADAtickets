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
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using TicketService = ADAtickets.ApiService.Services.TicketRepository;

namespace ADAtickets.ApiService.Tests.Services.TicketRepository
{
    /// <summary>
    /// <c>DeleteTicketByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    sealed public class DeleteTests
    {
        [Fact]
        public async Task DeleteTicketByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            var ticket = new Ticket { Id = Guid.NewGuid() };
            var tickets = new List<Ticket> { ticket };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = tickets.BuildMockDbSet();
            mockSet.Setup(s => s.Remove(It.IsAny<Ticket>()))
                .Callback<Ticket>(ticket => tickets.RemoveAll(t => t.Id == ticket.Id));
            mockContext.Setup(c => c.Tickets)
                .Returns(mockSet.Object);

            var service = new TicketService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteTicketAsync(ticket);
            var deletedTicket = await mockContext.Object.Tickets.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedTicket);
        }
    }
}