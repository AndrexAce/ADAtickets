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
using PlatformService = ADAtickets.ApiService.Services.PlatformRepository;

namespace ADAtickets.ApiService.Tests.Services.PlatformRepository
{
    /// <summary>
    /// <c>DeletePlatformByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    sealed public class DeleteTests
    {
        [Fact]
        public async Task GetPlatformByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var platform = new Platform { Id = guid };
            var platforms = new List<Platform> { platform };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = platforms.BuildMockDbSet();
            mockSet.Setup(s => s.Remove(It.IsAny<Platform>()))
                .Callback(() => platforms.RemoveAt(0));
            mockContext.Setup(c => c.Platforms)
                .Returns(mockSet.Object);

            var service = new PlatformService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeletePlatformAsync(platform);
            var deletedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedPlatform);
        }
    }
}