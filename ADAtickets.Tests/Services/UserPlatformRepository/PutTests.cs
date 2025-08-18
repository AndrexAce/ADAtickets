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

namespace ADAtickets.Tests.Services.UserPlatformRepository;

/// <summary>
///     <c>UpdateUserPlatform(UserPlatform)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public sealed class PutTests
{
    public static TheoryData<UserPlatform> InvalidUserPlatformData =>
    [
        // Entity with keys referring to no existing userPlatform or platform
        Utilities.CreateUserPlatform(Guid.NewGuid(), Guid.NewGuid()),
        // Entity already existing in the relation
        Utilities.CreateUserPlatform(Guid.Empty, Guid.Empty)
    ];

    public static TheoryData<UserPlatform> ValidUserPlatformData =>
    [
        Utilities.CreateUserPlatform(Guid.NewGuid(), Guid.NewGuid())
    ];

    [Theory]
    [MemberData(nameof(ValidUserPlatformData))]
    public async Task UpdateUserPlatform_ValidEntity_ReturnsNew(UserPlatform inUserPlatform)
    {
        // Arrange
        List<User> users = [new() { Id = inUserPlatform.UserId }];
        List<Platform> platforms = [new() { Id = Guid.Empty }, new() { Id = inUserPlatform.PlatformId }];
        List<UserPlatform> userPlatforms =
            [new() { Id = inUserPlatform.Id, UserId = inUserPlatform.UserId, PlatformId = Guid.Empty }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserPlatform>> mockUserPlatformSet = userPlatforms.BuildMockDbSet();
        _ = mockUserPlatformSet.Setup(s => s.Update(It.IsAny<UserPlatform>()))
            .Callback<UserPlatform>(up =>
            {
                if (users.Find(u => u.Id == up.UserId) is not null
                    && platforms.Find(p => p.Id == up.PlatformId) is not null
                    && userPlatforms.Find(u => u.UserId == up.UserId && u.PlatformId == up.PlatformId) is null)
                {
                    userPlatforms[0].UserId = inUserPlatform.UserId;
                    userPlatforms[0].PlatformId = inUserPlatform.PlatformId;
                }
            });
        _ = mockContext.Setup(c => c.UserPlatforms)
            .Returns(mockUserPlatformSet.Object);

        UserPlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserPlatformAsync(inUserPlatform);
        var updatedUserPlatform = await mockContext.Object.UserPlatforms.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUserPlatform);
        Assert.True(inUserPlatform.UserId == updatedUserPlatform.UserId &&
                    inUserPlatform.PlatformId == updatedUserPlatform.PlatformId);
    }

    [Theory]
    [MemberData(nameof(InvalidUserPlatformData))]
    public async Task UpdateUserPlatform_InvalidEntity_ReturnsOld(UserPlatform inUserPlatform)
    {
        // Arrange
        List<User> users = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];
        List<Platform> platforms = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];
        List<UserPlatform> userPlatforms =
        [
            new() { Id = inUserPlatform.Id, UserId = Guid.AllBitsSet, PlatformId = Guid.AllBitsSet },
            new() { Id = Guid.NewGuid(), UserId = Guid.Empty, PlatformId = Guid.Empty }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserPlatform>> mockUserPlatformSet = userPlatforms.BuildMockDbSet();
        _ = mockUserPlatformSet.Setup(s => s.Update(It.IsAny<UserPlatform>()))
            .Callback<UserPlatform>(up =>
            {
                if (users.Find(u => u.Id == up.UserId) is not null
                    && platforms.Find(p => p.Id == up.PlatformId) is not null
                    && userPlatforms.Find(u => u.UserId == up.UserId && u.PlatformId == up.PlatformId) is null)
                {
                    userPlatforms[0].UserId = inUserPlatform.UserId;
                    userPlatforms[0].PlatformId = inUserPlatform.PlatformId;
                }
            });
        _ = mockContext.Setup(c => c.UserPlatforms)
            .Returns(mockUserPlatformSet.Object);

        UserPlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserPlatformAsync(inUserPlatform);
        var updatedUserPlatform = await mockContext.Object.UserPlatforms.FirstOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUserPlatform);
        Assert.True(inUserPlatform.UserId != updatedUserPlatform.UserId ||
                    inUserPlatform.PlatformId != updatedUserPlatform.PlatformId);
    }
}