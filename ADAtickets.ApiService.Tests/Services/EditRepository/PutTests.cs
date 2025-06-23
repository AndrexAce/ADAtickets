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

namespace ADAtickets.ApiService.Tests.Services.EditRepository
{
    /// <summary>
    /// <c>UpdateEdit(Edit)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public partial class PutTests
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
        public async Task UpdateEdit_ValidEntity_ReturnsNew(Edit inEdit)
        {
            // Arrange
            var edits = new List<Edit> { new() { Id = inEdit.Id, Description = "Old description.", TicketId = Guid.AllBitsSet, UserId = Guid.AllBitsSet } };
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockEditSet = edits.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockEditSet.Setup(s => s.Update(It.IsAny<Edit>()))
                .Callback<Edit>(e =>
                {
                    if (e.Description.Length <= 200 && mockTicketSet.Object.Single().Id == e.TicketId && mockUserSet.Object.Single().Id == e.UserId)
                    {
                        edits[0].Description = inEdit.Description;
                    }
                });
            mockContext.Setup(c => c.Edits)
                .Returns(mockEditSet.Object);

            var service = new EditService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateEditAsync(inEdit);
            var updatedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedEdit);
            Assert.Equal(inEdit.Description, updatedEdit.Description);
        }

        [Theory]
        [MemberData(nameof(InvalidEditData))]
        public async Task UpdateEdit_InvalidEntity_ReturnsOld(Edit inEdit)
        {
            // Arrange
            var edits = new List<Edit> { new() { Id = inEdit.Id, Description = "Old description.", TicketId = Guid.AllBitsSet, UserId = Guid.AllBitsSet } };
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockEditSet = edits.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockEditSet.Setup(s => s.Update(It.IsAny<Edit>()))
                .Callback<Edit>(e =>
                {
                    if (e.Description.Length <= 200 && mockTicketSet.Object.Single().Id == e.TicketId && mockUserSet.Object.Single().Id == e.UserId)
                    {
                        edits[0].Description = inEdit.Description;
                    }
                });
            mockContext.Setup(c => c.Edits)
                .Returns(mockEditSet.Object);

            var service = new EditService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateEditAsync(inEdit);
            var updatedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedEdit);
            Assert.NotEqual(inEdit.Description, updatedEdit.Description);
        }
    }
}
