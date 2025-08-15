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
///     <c>AddUserPlatformAsync(UserPlatform)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public class PostTests
{
    public static TheoryData<UserPlatform> InvalidUserPlatformData =>
    [
        // Entity with keys referring to no existing user or platform
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
    public async Task AddUserPlatform_ValidEntity_ReturnsUserPlatform(UserPlatform inUserPlatform)
    {
        // Arrange
        List<User> users = [new() { Id = inUserPlatform.UserId }];
        List<Platform> platforms = [new() { Id = inUserPlatform.PlatformId }];
        List<UserPlatform> userPlatforms = [];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserPlatform>> mockUserPlatformSet = userPlatforms.BuildMockDbSet();
        _ = mockUserPlatformSet.Setup(s => s.Add(It.IsAny<UserPlatform>()))
            .Callback<UserPlatform>(up =>
            {
                if (users.Find(u => u.Id == up.UserId) is not null
                    && platforms.Find(p => p.Id == up.PlatformId) is not null
                    && userPlatforms.Find(u => u.UserId == up.UserId && u.PlatformId == up.PlatformId) is null)
                    userPlatforms.Add(up);
            });
        _ = mockContext.Setup(c => c.UserPlatforms)
            .Returns(mockUserPlatformSet.Object);

        UserPlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddUserPlatformAsync(inUserPlatform);
        var addedUserPlatform = await mockContext.Object.UserPlatforms.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(addedUserPlatform);
        Assert.NotEmpty(userPlatforms);
    }

    [Theory]
    [MemberData(nameof(InvalidUserPlatformData))]
    public async Task AddUserPlatform_InvalidEntity_ReturnsNothing(UserPlatform inUserPlatform)
    {
        // Arrange
        List<User> users = [new() { Id = Guid.Empty }];
        List<Platform> platforms = [new() { Id = Guid.Empty }];
        List<UserPlatform> userPlatforms = [new() { UserId = Guid.Empty, PlatformId = Guid.Empty }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserPlatform>> mockUserPlatformSet = userPlatforms.BuildMockDbSet();
        _ = mockUserPlatformSet.Setup(s => s.Add(It.IsAny<UserPlatform>()))
            .Callback<UserPlatform>(up =>
            {
                if (users.Find(u => u.Id == up.UserId) is not null
                    && platforms.Find(p => p.Id == up.PlatformId) is not null
                    && userPlatforms.Find(u => u.UserId == up.UserId && u.PlatformId == up.PlatformId) is null)
                    userPlatforms.Add(up);
            });
        _ = mockContext.Setup(c => c.UserPlatforms)
            .Returns(mockUserPlatformSet.Object);

        UserPlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddUserPlatformAsync(inUserPlatform);
        var addedUserPlatform = await mockContext.Object.UserPlatforms.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(addedUserPlatform);
        Assert.NotEmpty(userPlatforms);
    }
}