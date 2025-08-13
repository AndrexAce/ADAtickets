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
using NotificationService = ADAtickets.ApiService.Services.NotificationRepository;

namespace ADAtickets.ApiService.Tests.Services.NotificationRepository
{
    /// <summary>
    /// <c>GetNotificationByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetNotificationsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetNotificationByIdAsync_ExistingId_ReturnsNotification()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<Notification> notifications = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => notifications.Find(n => n.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            Notification? result = await service.GetNotificationByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetNotificationByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<Notification> notifications = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => notifications.Find(n => n.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            Notification? result = await service.GetNotificationByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetNotificationByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<Notification> notifications = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => notifications.Find(n => n.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            Notification? result = await service.GetNotificationByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetNotifications_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<Notification> notifications = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetNotifications_FullSet_ReturnsNotifications()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<Notification> notifications =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetNotificationsBy_OneFilterWithMatch_ReturnsNotifications()
        {
            // Arrange
            List<Notification> notifications =
            [
                new() { Message = "Example message." },
                new() { Message = "Trial message."},
                new() { Message = "Test message." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsByAsync([new KeyValuePair<string, string>("Message", "message")]);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains("message", result.ElementAt(0).Message, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("message", result.ElementAt(1).Message, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("message", result.ElementAt(2).Message, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetNotificationsBy_MoreFiltersWithMatch_ReturnNotifications()
        {
            // Arrange
            List<Notification> notifications =
            [
                new() { Message = "Example message.", SendDateTime = DateTimeOffset.UnixEpoch },
                new() { Message = "Trial message." },
                new() { Message = "Test message.", SendDateTime = DateTimeOffset.UnixEpoch }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsByAsync([
                new KeyValuePair<string, string>("Message", "message"),
                new KeyValuePair<string, string>("SendDateTime", DateTimeOffset.UnixEpoch.ToString())
                ]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("message", result.ElementAt(0).Message, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("message", result.ElementAt(1).Message, StringComparison.InvariantCultureIgnoreCase);
            Assert.True(DateTimeOffset.UnixEpoch <= result.ElementAt(0).SendDateTime);
            Assert.True(DateTimeOffset.UnixEpoch <= result.ElementAt(1).SendDateTime);
        }

        [Fact]
        public async Task GetNotificationsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<Notification> notifications =
            [
                new() { Message = "Example message." },
                new() { Message = "Trial message."},
                new() { Message = "Test message." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsByAsync([new KeyValuePair<string, string>("Message", "text")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsAll()
        {
            // Arrange
            List<Notification> notifications =
            [
                new() { Message = "Example description." },
                new() { Message = "Trial description."},
                new() { Message = "Test description." }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Notification>> mockSet = notifications.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            NotificationService service = new(mockContext.Object);

            // Act
            IEnumerable<Notification> result = await service.GetNotificationsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Equal(3, result.Count());
        }
        #endregion
    }
}