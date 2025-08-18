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
using EditService = ADAtickets.ApiService.Services.EditRepository;

namespace ADAtickets.Tests.Services.EditRepository;

/// <summary>
///     <c>AddEditAsync(Edit)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public sealed class PostTests
{
    public static TheoryData<Edit> InvalidEditData =>
    [
        Utilities.CreateEdit(new string('a', 201), Guid.AllBitsSet, Guid.AllBitsSet),
        Utilities.CreateEdit("Valid description.", Guid.Empty, Guid.AllBitsSet),
        Utilities.CreateEdit("Valid description.", Guid.AllBitsSet, Guid.Empty)
    ];

    public static TheoryData<Edit> ValidEditData =>
    [
        Utilities.CreateEdit("Valid description.", Guid.AllBitsSet, Guid.AllBitsSet)
    ];

    [Theory]
    [MemberData(nameof(ValidEditData))]
    public async Task AddEdit_ValidEntity_ReturnsEdit(Edit inEdit)
    {
        // Arrange
        List<Edit> edits = [];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Edit>> mockEditSet = edits.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockEditSet.Setup(s => s.Add(It.IsAny<Edit>()))
            .Callback<Edit>(e =>
            {
                if (e.Description.Length <= 200 && mockTicketSet.Object.Single().Id == e.TicketId &&
                    mockUserSet.Object.Single().Id == e.UserId) edits.Add(e);
            });
        _ = mockContext.Setup(c => c.Edits)
            .Returns(mockEditSet.Object);

        EditService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddEditAsync(inEdit);
        var addedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(addedEdit);
        Assert.NotEmpty(edits);
    }

    [Theory]
    [MemberData(nameof(InvalidEditData))]
    public async Task AddEdit_InvalidEntity_ReturnsNothing(Edit inEdit)
    {
        // Arrange
        List<Edit> edits = [];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Edit>> mockEditSet = edits.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockEditSet.Setup(s => s.Add(It.IsAny<Edit>()))
            .Callback<Edit>(e =>
            {
                if (e.Description.Length <= 200 && mockTicketSet.Object.Single().Id == e.TicketId &&
                    mockUserSet.Object.Single().Id == e.UserId) edits.Add(e);
            });
        _ = mockContext.Setup(c => c.Edits)
            .Returns(mockEditSet.Object);

        EditService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddEditAsync(inEdit);
        var addedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.Null(addedEdit);
        Assert.Empty(edits);
    }
}