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
using PlatformService = ADAtickets.ApiService.Services.PlatformRepository;

namespace ADAtickets.ApiService.Tests.Services.PlatformRepository
{
    /// <summary>
    /// <c>GetPlatformByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetPlatformsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetPlatformByIdAsync_ExistingId_ReturnsPlatform()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<Platform> platforms = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            Platform? result = await service.GetPlatformByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetPlatformByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<Platform> platforms = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            Platform? result = await service.GetPlatformByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPlatformByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<Platform> platforms = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            Platform? result = await service.GetPlatformByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetPlatforms_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<Platform> platforms = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPlatforms_FullSet_ReturnsPlatforms()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<Platform> platforms =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetPlatformsBy_OneFilterWithMatch_ReturnsPlatforms()
        {
            // Arrange
            List<Platform> platforms =
            [
                new() { RepositoryUrl = "https://example.com" },
                new() { RepositoryUrl = "https://trial.com"},
                new() { RepositoryUrl = "https://test.com" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsByAsync([new KeyValuePair<string, string>("RepositoryUrl", "https")]);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains("https", result.ElementAt(0).RepositoryUrl, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("https", result.ElementAt(1).RepositoryUrl, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("https", result.ElementAt(2).RepositoryUrl, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetPlatformsBy_MoreFiltersWithMatch_ReturnPlatforms()
        {
            // Arrange
            List<Platform> platforms =
            [
                new() { Name = "Example repo", RepositoryUrl = "https://example.com" },
                new() { Name = "Trial repo", RepositoryUrl = "https://trial.com"},
                new() { Name = "Test repo", RepositoryUrl = "https://test.com" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsByAsync([
                new KeyValuePair<string, string>("Name", "repo"),
                new KeyValuePair<string, string>("RepositoryUrl", "https://t")
                ]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("repo", result.ElementAt(0).Name, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("repo", result.ElementAt(1).Name, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("https://t", result.ElementAt(0).RepositoryUrl, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("https://t", result.ElementAt(1).RepositoryUrl, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetPlatformsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<Platform> platforms =
            [
                new() { RepositoryUrl = "https://example.com" },
                new() { RepositoryUrl = "https://trial.com"},
                new() { RepositoryUrl = "https://test.com" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsByAsync([new KeyValuePair<string, string>("RepositoryUrl", ".it")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsAll()
        {
            // Arrange
            List<Platform> platforms =
            [
                new() { RepositoryUrl = "https://example.com" },
                new() { RepositoryUrl = "https://trial.com"},
                new() { RepositoryUrl = "https://test.com" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            PlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<Platform> result = await service.GetPlatformsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Equal(3, result.Count());
        }
        #endregion
    }
}