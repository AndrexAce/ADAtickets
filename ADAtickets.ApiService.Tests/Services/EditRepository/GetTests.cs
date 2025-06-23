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
    sealed public class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetEditByIdAsync_ExistingId_ReturnsEdit()
        {
            // Arrange
            var existingId = Guid.NewGuid();

            var edits = new List<Edit> { new() { Id = existingId } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetEditByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var edits = new List<Edit> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEditByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            var edits = new List<Edit> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => edits.Find(e => e.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetEdits_EmptySet_ReturnsNothing()
        {
            // Arrange
            var edits = new List<Edit>();

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEdits_FullSet_ReturnsEdits()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            var edits = new List<Edit> {
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsAsync();

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
            var edits = new List<Edit> {
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsByAsync([new KeyValuePair<string, string>("Description", "description")]);

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
            var edits = new List<Edit> {
                new() { Description = "Example description.", EditDateTime = DateTimeOffset.UnixEpoch },
                new() { Description = "Trial description." },
                new() { Description = "Test description.", EditDateTime = DateTimeOffset.UnixEpoch }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsByAsync([
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
            var edits = new List<Edit> {
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsByAsync([new KeyValuePair<string, string>("Description", "text")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsNothing()
        {
            // Arrange
            var edits = new List<Edit> {
                new() { Description = "Example description." },
                new() { Description = "Trial description."},
                new() { Description = "Test description." }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = edits.BuildMockDbSet();
            mockContext.Setup(c => c.Edits)
                .Returns(mockSet.Object);

            var service = new EditService(mockContext.Object);

            // Act
            var result = await service.GetEditsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Empty(result);
        }
        #endregion
    }
}