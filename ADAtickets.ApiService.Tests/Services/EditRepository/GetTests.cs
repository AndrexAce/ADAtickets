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
using EditService = ADAtickets.ApiService.Services.EditRepository;

namespace ADAtickets.ApiService.Tests.Services.EditRepository
{
    /// <summary>
    /// <c>GetEditByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetEditsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetEditByIdAsync_ExistingId_ReturnsEdit()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<Edit> edits = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            Edit? result = await service.GetEditByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetEditByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<Edit> edits = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            Edit? result = await service.GetEditByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEditByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<Edit> edits = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            Edit? result = await service.GetEditByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetEdits_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<Edit> edits = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEdits_FullSet_ReturnsEdits()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<Edit> edits =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetEditsBy_OneFilterWithMatch_ReturnsEdits()
        {
            // Arrange
            List<Edit> edits =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsByAsync([new KeyValuePair<string, string>("Description", "description")]);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains("description", result.ElementAt(0).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(1).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(2).Description, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetEditsBy_MoreFiltersWithMatch_ReturnEdits()
        {
            // Arrange
            List<Edit> edits =
            [
                new() { Description = "Example description.", EditDateTime = DateTimeOffset.UnixEpoch },
                new() { Description = "Trial description." },
                new() { Description = "Test description.", EditDateTime = DateTimeOffset.UnixEpoch }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsByAsync([
                new KeyValuePair<string, string>("Description", "description"),
                new KeyValuePair<string, string>("EditDateTime", DateTimeOffset.UnixEpoch.ToString())
                ]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("description", result.ElementAt(0).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("description", result.ElementAt(1).Description, StringComparison.InvariantCultureIgnoreCase);
            Assert.True(DateTimeOffset.UnixEpoch <= result.ElementAt(0).EditDateTime);
            Assert.True(DateTimeOffset.UnixEpoch <= result.ElementAt(1).EditDateTime);
        }

        [Fact]
        public async Task GetEditsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<Edit> edits =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsByAsync([new KeyValuePair<string, string>("Description", "text")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsAll()
        {
            // Arrange
            List<Edit> edits =
            [
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Edit>> mockSet = edits.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            EditService service = new(mockContext.Object);

            // Act
            IEnumerable<Edit> result = await service.GetEditsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Equal(3, result.Count());
        }
        #endregion
    }
}