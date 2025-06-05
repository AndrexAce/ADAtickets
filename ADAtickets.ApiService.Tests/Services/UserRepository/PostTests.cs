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
using UserService = ADAtickets.ApiService.Services.UserRepository;

namespace ADAtickets.ApiService.Tests.Services.UserRepository
{
    /// <summary>
    /// <c>AddUserAsync(User)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PostTests
    {
        public static TheoryData<User> InvalidUserData =>
        [
            Utilities.CreateUser(name: new string('a', 51), surname: "Lucchese", microsoftAccountId: "someId"),
            Utilities.CreateUser(name: new string('a', 51), surname: "Lucchese", microsoftAccountId: null),
            Utilities.CreateUser(name: "Andrea", surname: new string('a', 51), microsoftAccountId: "someId"),
            Utilities.CreateUser(name: "Andrea", surname: new string('a', 51), microsoftAccountId: null),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: new string('a', 21))
        ];

        public static TheoryData<User> ValidUserData =>
        [
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: "someId"),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: null)
        ];

        [Theory]
        [MemberData(nameof(ValidUserData))]
        public async Task AddUser_ValidEntity_ReturnsUser(User inUser)
        {
            // Arrange
            var users = new List<User>();

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockUserSet = users.BuildMockDbSet();
            mockUserSet.Setup(s => s.Add(It.IsAny<User>()))
                .Callback<User>(u =>
                {
                    if (u.Name.Length <= 50 && u.Surname.Length <= 50
                    && (u.MicrosoftAccountId == null || u.MicrosoftAccountId.Length <= 20))
                    {
                        users.Add(u);
                    }
                });
            mockContext.Setup(c => c.Users)
                .Returns(mockUserSet.Object);

            var service = new UserService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddUserAsync(inUser);
            var addedUser = await mockContext.Object.Users.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(addedUser);
            Assert.NotEmpty(users);
        }

        [Theory]
        [MemberData(nameof(InvalidUserData))]
        public async Task AddUser_InvalidEntity_ReturnsNothing(User inUser)
        {
            // Arrange
            var users = new List<User>();

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockUserSet = users.BuildMockDbSet();
            mockUserSet.Setup(s => s.Add(It.IsAny<User>()))
                .Callback<User>(u =>
                {
                    if (u.Name.Length <= 50 && u.Surname.Length <= 50
                    && (u.MicrosoftAccountId == null || u.MicrosoftAccountId.Length <= 20))
                    {
                        users.Add(u);
                    }
                });
            mockContext.Setup(c => c.Users)
                .Returns(mockUserSet.Object);

            var service = new UserService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddUserAsync(inUser);
            var addedUser = await mockContext.Object.Users.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(addedUser);
            Assert.Empty(users);
        }
    }
}
