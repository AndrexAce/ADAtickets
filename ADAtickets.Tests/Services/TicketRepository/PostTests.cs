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
using TicketService = ADAtickets.ApiService.Services.TicketRepository;

namespace ADAtickets.Tests.Services.TicketRepository;

/// <summary>
///     <c>AddTicketAsync(Ticket)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public sealed class PostTests
{
    public static TheoryData<Ticket> InvalidTicketData =>
    [
        Utilities.CreateTicket(new string('a', 51), "Description.", Guid.AllBitsSet, Guid.AllBitsSet, null),
        Utilities.CreateTicket(new string('a', 51), "Description.", Guid.AllBitsSet, Guid.AllBitsSet, Guid.Empty),
        Utilities.CreateTicket("Title.", new string('a', 5001), Guid.AllBitsSet, Guid.AllBitsSet, null),
        Utilities.CreateTicket("Title.", new string('a', 5001), Guid.AllBitsSet, Guid.AllBitsSet, Guid.Empty),
        Utilities.CreateTicket("Title.", "Description.", Guid.Empty, Guid.AllBitsSet, null),
        Utilities.CreateTicket("Title.", "Description.", Guid.Empty, Guid.AllBitsSet, Guid.Empty),
        Utilities.CreateTicket("Title.", "Description.", Guid.AllBitsSet, Guid.NewGuid(), null),
        Utilities.CreateTicket("Title.", "Description.", Guid.AllBitsSet, Guid.NewGuid(), Guid.Empty),
        Utilities.CreateTicket("Title.", "Description.", Guid.AllBitsSet, Guid.AllBitsSet, Guid.NewGuid())
    ];

    public static TheoryData<Ticket> ValidTicketData =>
    [
        Utilities.CreateTicket("Title.", "Description.", Guid.AllBitsSet, Guid.AllBitsSet, null),
        Utilities.CreateTicket("Title.", "Description.", Guid.AllBitsSet, Guid.AllBitsSet, Guid.Empty)
    ];

    [Theory]
    [MemberData(nameof(ValidTicketData))]
    public async Task AddTicket_ValidEntity_ReturnsTicket(Ticket inTicket)
    {
        // Arrange
        List<Ticket> tickets = [];
        List<Platform> platform = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<Platform>> mockPlatformSet = platform.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockTicketSet.Setup(s => s.Add(It.IsAny<Ticket>()))
            .Callback<Ticket>(t =>
            {
                if (t.Title.Length <= 50 && t.Description.Length <= 5000
                                         && mockPlatformSet.Object.Single().Id == t.PlatformId
                                         && mockUserSet.Object.ElementAt(0).Id == t.CreatorUserId
                                         && (t.OperatorUserId == null ||
                                             mockUserSet.Object.ElementAt(1).Id == t.OperatorUserId))
                    tickets.Add(t);
            });
        _ = mockContext.Setup(c => c.Tickets)
            .Returns(mockTicketSet.Object);

        TicketService service = new(mockContext.Object);

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
        List<Ticket> tickets = [];
        List<Platform> platform = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<Platform>> mockPlatformSet = platform.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockTicketSet.Setup(s => s.Add(It.IsAny<Ticket>()))
            .Callback<Ticket>(t =>
            {
                if (t.Title.Length <= 50 && t.Description.Length <= 5000
                                         && mockPlatformSet.Object.Single().Id == t.PlatformId
                                         && mockUserSet.Object.ElementAt(0).Id == t.CreatorUserId
                                         && (t.OperatorUserId == null ||
                                             mockUserSet.Object.ElementAt(1).Id == t.OperatorUserId))
                    tickets.Add(t);
            });
        _ = mockContext.Setup(c => c.Tickets)
            .Returns(mockTicketSet.Object);

        TicketService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddTicketAsync(inTicket);
        var addedTicket = await mockContext.Object.Tickets.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.Null(addedTicket);
        Assert.Empty(tickets);
    }
}