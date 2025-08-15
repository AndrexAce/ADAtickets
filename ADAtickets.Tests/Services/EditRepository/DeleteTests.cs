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

namespace ADAtickets.Tests.Services.EditRepository
{
    /// <summary>
    /// <c>DeleteEditByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    public sealed class DeleteTests
    {
        [Fact]
        public async Task DeleteEditByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            Edit edit = new() { Id = Guid.NewGuid() };
            List<Edit> edits = [edit];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockSet.Setup(s => s.Remove(It.IsAny<Edit>()))
                .Callback<Edit>(edit => edits.RemoveAll(e => e.Id == edit.Id));
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteEditAsync(edit);
            Edit? deletedEdit = await mockContext.Object.Edits.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedEdit);
        }
    }
}