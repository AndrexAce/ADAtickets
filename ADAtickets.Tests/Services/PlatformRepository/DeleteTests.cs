﻿/*
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
using PlatformService = ADAtickets.ApiService.Services.PlatformRepository;

namespace ADAtickets.Tests.Services.PlatformRepository;

/// <summary>
///     <c>DeletePlatformByIdAsync(Guid)</c>
///     <list type="number">
///         <item>Existing entity</item>
///         <item>Non existing entity</item>
///     </list>
/// </summary>
public sealed class DeleteTests
{
    [Fact]
    public async Task DeletePlatformByIdAsync_ExistingEntity_DeletesEntity()
    {
        // Arrange
        Platform platform = new() { Id = Guid.NewGuid() };
        List<Platform> platforms = [platform];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
        _ = mockSet.Setup(s => s.Remove(It.IsAny<Platform>()))
            .Callback<Platform>(platform => platforms.RemoveAll(p => p.Id == platform.Id));
        _ = mockContext.Setup(c => c.Platforms)
            .Returns(mockSet.Object);

        PlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.DeletePlatformAsync(platform);
        var deletedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.Null(deletedPlatform);
    }

    [Fact]
    public async Task DeletePlatformByIdAsync_NonExistingEntity_DoesNothing()
    {
        // Arrange
        Platform platform = new() { Id = Guid.NewGuid() };
        List<Platform> platforms = [new() { Id = Guid.NewGuid() }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Platform>> mockSet = platforms.BuildMockDbSet();
        _ = mockSet.Setup(s => s.Remove(It.IsAny<Platform>()))
            .Callback<Platform>(platform => platforms.RemoveAll(p => p.Id == platform.Id));
        _ = mockContext.Setup(c => c.Platforms)
            .Returns(mockSet.Object);

        PlatformService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.DeletePlatformAsync(platform);
        var deletedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(deletedPlatform);
    }
}