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
using UserService = ADAtickets.ApiService.Services.UserRepository;

namespace ADAtickets.Tests.Services.UserRepository;

/// <summary>
///     <c>GetUserByIdAsync(Guid)</c>
///     <list type="number">
///         <item>Existing id</item>
///         <item>Non-existing id</item>
///         <item>Empty id</item>
///     </list>
///     <c>GetUsersAsync()</c>
///     <list type="number">
///         <item>Empty set</item>
///         <item>Full set</item>
///     </list>
/// </summary>
public sealed class GetTests
{
    #region GetOne

    [Fact]
    public async Task GetUserByIdAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        var existingId = Guid.NewGuid();

        List<User> users = [new() { Id = existingId }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
            .ReturnsAsync((object[] arguments) => users.Find(u => u.Id == (Guid)arguments[0]));
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUserByIdAsync(existingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingId, result.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        List<User> users = [new() { Id = Guid.NewGuid() }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
            .ReturnsAsync((object[] arguments) => users.Find(u => u.Id == (Guid)arguments[0]));
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_EmptyId_ReturnsNull()
    {
        // Arrange
        List<User> users = [new() { Id = Guid.NewGuid() }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
            .ReturnsAsync((object[] arguments) => users.Find(u => u.Id == (Guid)arguments[0]));
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUserByIdAsync(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetUsers_EmptySet_ReturnsNothing()
    {
        // Arrange
        List<User> users = [];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUsers_FullSet_ReturnsUsers()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();

        List<User> users =
        [
            new() { Id = guid1 },
            new() { Id = guid2 },
            new() { Id = guid3 }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal(guid1, result.ElementAt(0).Id);
        Assert.Equal(guid2, result.ElementAt(1).Id);
        Assert.Equal(guid3, result.ElementAt(2).Id);
    }

    #endregion

    #region GetBy

    [Fact]
    public async Task GetUsersBy_OneFilterWithMatch_ReturnsUsers()
    {
        // Arrange
        List<User> users =
        [
            new() { Name = "Jonathan" },
            new() { Name = "Jack" },
            new() { Name = "James" }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersByAsync([new KeyValuePair<string, string>("Name", "j")]);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains("j", result.ElementAt(0).Name, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("j", result.ElementAt(1).Name, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("j", result.ElementAt(2).Name, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task GetUsersBy_MoreFiltersWithMatch_ReturnUsers()
    {
        // Arrange
        List<User> users =
        [
            new() { Name = "Jonathan", Type = UserType.Admin },
            new() { Name = "Jack" },
            new() { Name = "James", Type = UserType.Admin }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersByAsync([
            new KeyValuePair<string, string>("Name", "j"),
            new KeyValuePair<string, string>("Type", UserType.Admin.ToString())
        ]);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains("j", result.ElementAt(0).Name, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("j", result.ElementAt(1).Name, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal(UserType.Admin, result.ElementAt(0).Type);
        Assert.Equal(UserType.Admin, result.ElementAt(1).Type);
    }

    [Fact]
    public async Task GetUsersBy_NoMatch_ReturnsNothing()
    {
        // Arrange
        List<User> users =
        [
            new() { Name = "Jonathan" },
            new() { Name = "Jack" },
            new() { Name = "James" }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersByAsync([new KeyValuePair<string, string>("Name", "i")]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAttachmentsBy_InvalidFilter_ReturnsAll()
    {
        // Arrange
        List<User> users =
        [
            new() { Name = "Jonathan" },
            new() { Name = "Jack" },
            new() { Name = "James" }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockSet = users.BuildMockDbSet();
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockSet.Object);

        UserService service = new(mockContext.Object);

        // Act
        var result = await service.GetUsersByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

        // Assert
        Assert.Equal(3, result.Count());
    }

    #endregion
}