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
using System.Text.RegularExpressions;
using UserService = ADAtickets.ApiService.Services.UserRepository;

namespace ADAtickets.Tests.Services.UserRepository;

/// <summary>
///     <c>UpdateUser(User)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public sealed class PutTests
{
    public static TheoryData<User> InvalidUserData =>
    [
        Utilities.CreateUser("@gmail.com", "AndrexAce", "Andrea", "Lucchese"),
        Utilities.CreateUser("test@gmail.com", new string('a', 51), "Andrea", "Lucchese"),
        Utilities.CreateUser("test@gmail.com", "AndrexAce", new string('a', 51), "Lucchese"),
        Utilities.CreateUser("test@gmail.com", "AndrexAce", "Andrea", new string('a', 51))
    ];

    public static TheoryData<User> ValidUserData =>
    [
        Utilities.CreateUser("andrylook14@gmail.com", "AndrexAce", "Andrea", "Lucchese")
    ];

    [Theory]
    [MemberData(nameof(ValidUserData))]
    public async Task UpdateUser_ValidEntity_ReturnsNew(User inUser)
    {
        // Arrange
        List<User> users = [new() { Id = inUser.Id, Email = "test@gmail.com", Username = "Andrews", Name = "Andrew", Surname = "Turchese" }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockUserSet.Setup(s => s.Update(It.IsAny<User>()))
            .Callback<User>(u =>
            {
                if (Regex.IsMatch(u.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)) &&
                    u.Username.Length <= 50 && u.Name.Length <= 50 && u.Surname.Length <= 50)
                {
                    users[0].Email = inUser.Email;
                    users[0].Username = inUser.Username;
                    users[0].Name = inUser.Name;
                    users[0].Surname = inUser.Surname;
                }
            });
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockUserSet.Object);

        UserService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserAsync(inUser);
        var updatedUser = await mockContext.Object.Users.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal(inUser.Email, updatedUser.Email);
        Assert.Equal(inUser.Username, updatedUser.Username);
        Assert.Equal(inUser.Name, updatedUser.Name);
        Assert.Equal(inUser.Surname, updatedUser.Surname);
    }

    [Theory]
    [MemberData(nameof(InvalidUserData))]
    public async Task UpdateUser_InvalidEntity_ReturnsOld(User inUser)
    {
        // Arrange
        List<User> users = [new() { Id = inUser.Id, Email = "old@gmail.com", Username = "Andrews", Name = "Andrew", Surname = "Turchese" }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockUserSet.Setup(s => s.Update(It.IsAny<User>()))
            .Callback<User>(u =>
            {
                if (Regex.IsMatch(u.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)) &&
                    u.Username.Length <= 50 && u.Name.Length <= 50 && u.Surname.Length <= 50)
                {
                    users[0].Email = inUser.Email;
                    users[0].Username = inUser.Username;
                    users[0].Name = inUser.Name;
                    users[0].Surname = inUser.Surname;
                }
            });
        _ = mockContext.Setup(c => c.Users)
            .Returns(mockUserSet.Object);

        UserService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserAsync(inUser);
        var updatedUser = await mockContext.Object.Users.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.NotEqual(inUser.Email, updatedUser.Email);
        Assert.NotEqual(inUser.Username, updatedUser.Username);
        Assert.NotEqual(inUser.Name, updatedUser.Name);
        Assert.NotEqual(inUser.Surname, updatedUser.Surname);
    }
}