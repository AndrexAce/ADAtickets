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
using EditService = ADAtickets.ApiService.Services.EditRepository;

namespace ADAtickets.ApiService.Tests.Services.EditRepository
{
    /// <summary>
    /// <c>AddEditAsync(Edit)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public partial class PostTests
    {
        public static TheoryData<Edit> InvalidEditData =>
        [
            Utilities.CreateEdit(description: new string('a', 201), ticketId: Guid.AllBitsSet, userId: Guid.AllBitsSet),
            Utilities.CreateEdit(description: "Valid description.", ticketId: Guid.Empty, userId : Guid.AllBitsSet),
            Utilities.CreateEdit(description: "Valid description.", ticketId: Guid.AllBitsSet, userId: Guid.Empty),
        ];

        public static TheoryData<Edit> ValidEditData =>
        [
            Utilities.CreateEdit(description: "Valid description.", ticketId: Guid.AllBitsSet, userId: Guid.AllBitsSet)
        ];

        [Theory]
        [MemberData(nameof(ValidEditData))]
        public async Task AddEdit_ValidEntity_ReturnsEdit(Edit inEdit)
        {
            // Arrange
            var edits = new List<Edit>();
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockEditSet = edits.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockEditSet.Setup(s => s.Add(It.IsAny<Edit>()))
                .Callback<Edit>(a =>
                {
                    if (a.Description.Length <= 200 && mockTicketSet.Object.Single().Id == a.TicketId && mockUserSet.Object.Single().Id == a.UserId)
                    {
                        edits.Add(a);
                    }
                });
            mockContext.Setup(c => c.Edits)
                .Returns(mockEditSet.Object);

            var service = new EditService(mockContext.Object);

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
            var edits = new List<Edit>();
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockEditSet = edits.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockEditSet.Setup(s => s.Add(It.IsAny<Edit>()))
                .Callback<Edit>(a =>
                {
                    if (a.Description.Length <= 200 && mockTicketSet.Object.Single().Id == a.TicketId && mockUserSet.Object.Single().Id == a.UserId)
                    {
                        edits.Add(a);
                    }
                });
            mockContext.Setup(c => c.Edits)
                .Returns(mockEditSet.Object);

            var service = new EditService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddEditAsync(inEdit);
            var addedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(addedEdit);
            Assert.Empty(edits);
        }
    }
}
