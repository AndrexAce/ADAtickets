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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using UserService = ADAtickets.ApiService.Services.UserRepository;

namespace ADAtickets.ApiService.Tests.Services.UserRepository
{
    /// <summary>
    /// <c>UpdateUser(User)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PutTests
    {
        public static TheoryData<User> InvalidUserData =>
        [
            Utilities.CreateUser(name: new string('a', 51), surname: "Lucchese", microsoftAccountId: "someId", identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: new string('a', 51), surname: "Lucchese", microsoftAccountId: null, identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: "Andrea", surname: new string('a', 51), microsoftAccountId: "someId", identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: "Andrea", surname: new string('a', 51), microsoftAccountId: null, identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: new string('a', 21), identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: "someId", identityUserId: Guid.Empty),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: null, identityUserId: Guid.Empty),
        ];

        public static TheoryData<User> ValidUserData =>
        [
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: "someId", identityUserId: Guid.AllBitsSet),
            Utilities.CreateUser(name: "Andrea", surname: "Lucchese", microsoftAccountId: null, identityUserId: Guid.AllBitsSet)
        ];

        [Theory]
        [MemberData(nameof(ValidUserData))]
        public async Task UpdateUser_ValidEntity_ReturnsNew(User inUser)
        {
            // Arrange
            var users = new List<User> { new() { Id = inUser.Id, Name = "Andrew", Surname = "Turchese", IdentityUserId = Guid.AllBitsSet } };
            var identityUsers = new List<IdentityUser<Guid>> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockUserSet = users.BuildMockDbSet();
            var mockIdentityUserSet = identityUsers.BuildMockDbSet();
            mockUserSet.Setup(s => s.Update(It.IsAny<User>()))
                .Callback<User>(u =>
                {
                    if (u.Name.Length <= 50 && u.Surname.Length <= 50
                    && (u.MicrosoftAccountId == null || u.MicrosoftAccountId.Length <= 20)
                    && mockIdentityUserSet.Object.Single().Id == u.IdentityUserId)
                    {
                        users[0].Name = inUser.Name;
                        users[0].Surname = inUser.Surname;
                    }
                });
            mockContext.Setup(c => c.AppUsers)
                .Returns(mockUserSet.Object);

            var service = new UserService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateUserAsync(inUser);
            var updatedUser = await mockContext.Object.AppUsers.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal(inUser.Name, updatedUser.Name);
            Assert.Equal(inUser.Surname, updatedUser.Surname);
        }

        [Theory]
        [MemberData(nameof(InvalidUserData))]
        public async Task UpdateUser_InvalidEntity_ReturnsOld(User inUser)
        {
            // Arrange
            var users = new List<User> { new() { Id = inUser.Id, Name = "Andrew", Surname = "Turchese", IdentityUserId = Guid.AllBitsSet } };
            var identityUsers = new List<IdentityUser<Guid>> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockUserSet = users.BuildMockDbSet();
            var mockIdentityUserSet = identityUsers.BuildMockDbSet();
            mockUserSet.Setup(s => s.Update(It.IsAny<User>()))
                .Callback<User>(u =>
                {
                    if (u.Name.Length <= 50 && u.Surname.Length <= 50
                    && (u.MicrosoftAccountId == null || u.MicrosoftAccountId.Length <= 20)
                    && mockIdentityUserSet.Object.Single().Id == u.IdentityUserId)
                    {
                        users[0].Name = inUser.Name;
                        users[0].Surname = inUser.Surname;
                    }
                });
            mockContext.Setup(c => c.AppUsers)
                .Returns(mockUserSet.Object);

            var service = new UserService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateUserAsync(inUser);
            var updatedUser = await mockContext.Object.AppUsers.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.NotEqual(inUser.Name, updatedUser.Name);
            Assert.NotEqual(inUser.Surname, updatedUser.Surname);
        }
    }
}
