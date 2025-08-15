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
using UserPlatformService = ADAtickets.ApiService.Services.UserPlatformRepository;

namespace ADAtickets.Tests.Services.UserPlatformRepository
{
    /// <summary>
    /// <c>DeleteUserPlatformByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    public sealed class DeleteTests
    {
        [Fact]
        public async Task DeleteUserByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            User user = new() { Id = Guid.NewGuid() };
            Platform platform = new() { Id = Guid.NewGuid() };
            UserPlatform userPlatform = new() { Id = Guid.NewGuid(), UserId = user.Id, PlatformId = platform.Id };
            List<UserPlatform> userPlatforms = [userPlatform];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<UserPlatform>> mockSet = userPlatforms.BuildMockDbSet();
            _ = mockSet.Setup(s => s.Remove(It.IsAny<UserPlatform>()))
                .Callback<UserPlatform>(userPlatform => userPlatforms.RemoveAll(up => up.Id == userPlatform.Id));
            _ = mockContext.Setup(c => c.UserPlatforms)
                .Returns(mockSet.Object);

            UserPlatformService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteUserPlatformAsync(userPlatform);
            UserPlatform? deletedUserPlatform = await mockContext.Object.UserPlatforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedUserPlatform);
        }
    }
}