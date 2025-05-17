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
    sealed public class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetPlatformByIdAsync_ExistingId_ReturnsPlatform()
        {
            // Arrange
            var existingId = Guid.NewGuid();

            var platforms = new List<Platform> { new() { Id = existingId } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            // Act
            var result = await service.GetPlatformByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetPlatformByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var platforms = new List<Platform> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            // Act
            var result = await service.GetPlatformByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPlatformByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            var platforms = new List<Platform> { new() { Id = Guid.NewGuid() } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => platforms.Find(p => p.Id == (Guid)arguments[0]));
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            // Act
            var result = await service.GetPlatformByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetPlatforms_EmptySet_ReturnsNothing()
        {
            // Arrange
            var platforms = new List<Platform>();

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            // Act
            var result = await service.GetPlatformsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPlatforms_FullSet_ReturnsPlatforms()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            var platforms = new List<Platform> {
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            // Act
            var result = await service.GetPlatformsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion
    }
}