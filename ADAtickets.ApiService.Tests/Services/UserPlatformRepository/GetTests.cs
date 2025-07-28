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
using UserPlatformService = ADAtickets.ApiService.Services.UserPlatformRepository;

namespace ADAtickets.ApiService.Tests.Services.UserPlatformRepository
{
    /// <summary>
    /// <c>GetUserPlatformByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetUserPlatformsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetUserPlatformByIdAsync_ExistingId_ReturnsUserPlatform()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<UserPlatform> userPlatforms = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userPlatforms.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            UserPlatform? result = await service.GetUserPlatformByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetUserPlatformByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<UserPlatform> userPlatforms = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userPlatforms.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            UserPlatform? result = await service.GetUserPlatformByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserPlatformByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<UserPlatform> userPlatforms = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userPlatforms.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            UserPlatform? result = await service.GetUserPlatformByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetUserPlatforms_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<UserPlatform> userPlatforms = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserPlatforms_FullSet_ReturnsUserPlatforms()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<UserPlatform> userPlatforms =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetUserPlatformsBy_OneFilterWithMatch_ReturnsUserPlatforms()
        {
            // Arrange
            Guid platformId = Guid.NewGuid();

            List<UserPlatform> userPlatforms =
            [
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = Guid.NewGuid(), PlatformId = platformId },
                new() { UserId = Guid.NewGuid(), PlatformId = platformId }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsByAsync([new KeyValuePair<string, string>("PlatformId", platformId.ToString())]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal(platformId, result.ElementAt(0).PlatformId);
            Assert.Equal(platformId, result.ElementAt(1).PlatformId);
        }

        [Fact]
        public async Task GetUserPlatformsBy_MoreFiltersWithMatch_ReturnUserPlatforms()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid platformId = Guid.NewGuid();

            List<UserPlatform> userPlatforms =
            [
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = userId, PlatformId = platformId },
                new() { UserId = Guid.NewGuid(), PlatformId = platformId }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsByAsync([
                new KeyValuePair<string, string>("PlatformId", platformId.ToString()),
                new KeyValuePair<string, string>("UserId", userId.ToString())
                ]);

            // Assert
            _ = Assert.Single(result);
            Assert.Equal(userId, result.ElementAt(0).UserId);
            Assert.Equal(platformId, result.ElementAt(0).PlatformId);
        }

        [Fact]
        public async Task GetUserPlatformsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<UserPlatform> userPlatforms =
            [
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsByAsync([new KeyValuePair<string, string>("PlatformId", Guid.NewGuid().ToString())]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsNothing()
        {
            // Arrange
            List<UserPlatform> userPlatforms =
            [
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() },
                new() { UserId = Guid.NewGuid(), PlatformId = Guid.NewGuid() }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            // Act
            IEnumerable<UserPlatform> result = await service.GetUserPlatformsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Empty(result);
        }
        #endregion
    }
}