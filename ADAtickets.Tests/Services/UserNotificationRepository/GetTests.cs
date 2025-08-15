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
using MockQueryable.Moq;
using Moq;
using UserNotificationService = ADAtickets.ApiService.Services.UserNotificationRepository;

namespace ADAtickets.Tests.Services.UserNotificationRepository
{
    /// <summary>
    /// <c>GetUserNotificationByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetUserNotificationsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetUserNotificationByIdAsync_ExistingId_ReturnsUserNotification()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<UserNotification> userNotifications = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userNotifications.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            UserNotification? result = await service.GetUserNotificationByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetUserNotificationByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<UserNotification> userNotifications = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userNotifications.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            UserNotification? result = await service.GetUserNotificationByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserNotificationByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<UserNotification> userNotifications = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => userNotifications.Find(u => u.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            UserNotification? result = await service.GetUserNotificationByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetUserNotifications_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<UserNotification> userNotifications = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserNotifications_FullSet_ReturnsUserNotifications()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<UserNotification> userNotifications =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetUserNotificationsBy_OneFilterWithMatch_ReturnsUserNotifications()
        {
            // Arrange
            Guid notificationId = Guid.NewGuid();

            List<UserNotification> userNotifications =
            [
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = notificationId },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = notificationId }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsByAsync([new KeyValuePair<string, string>("NotificationId", notificationId.ToString())]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal(notificationId, result.ElementAt(0).NotificationId);
            Assert.Equal(notificationId, result.ElementAt(1).NotificationId);
        }

        [Fact]
        public async Task GetUserNotificationsBy_MoreFiltersWithMatch_ReturnUserNotifications()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid notificationId = Guid.NewGuid();

            List<UserNotification> userNotifications =
            [
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = userId, NotificationId = notificationId },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = notificationId }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsByAsync([
                new KeyValuePair<string, string>("NotificationId", notificationId.ToString()),
                new KeyValuePair<string, string>("ReceiverUserId", userId.ToString())
                ]);

            // Assert
            _ = Assert.Single(result);
            Assert.Equal(userId, result.ElementAt(0).ReceiverUserId);
            Assert.Equal(notificationId, result.ElementAt(0).NotificationId);
        }

        [Fact]
        public async Task GetUserNotificationsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<UserNotification> userNotifications =
            [
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsByAsync([new KeyValuePair<string, string>("NotificationId", Guid.NewGuid().ToString())]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsAll()
        {
            // Arrange
            List<UserNotification> userNotifications =
            [
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() },
                new() { ReceiverUserId = Guid.NewGuid(), NotificationId = Guid.NewGuid() }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<UserNotification>> mockSet = userNotifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.UserNotifications)
                .Returns(mockSet.Object);

            UserNotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<UserNotification> result = await service.GetUserNotificationsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Equal(3, result.Count());
        }
        #endregion
    }
}