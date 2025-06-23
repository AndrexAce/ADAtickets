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
using NotificationService = ADAtickets.ApiService.Services.NotificationRepository;

namespace ADAtickets.ApiService.Tests.Services.NotificationRepository
{
    /// <summary>
    /// <c>DeleteNotificationByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    sealed public class DeleteTests
    {
        [Fact]
        public async Task DeleteNotificationByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            var notification = new Notification { Id = Guid.NewGuid() };
            var notifications = new List<Notification> { notification };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = notifications.BuildMockDbSet();
            mockSet.Setup(s => s.Remove(It.IsAny<Notification>()))
                .Callback<Notification>(notification => notifications.RemoveAll(n => n.Id == notification.Id));
            mockContext.Setup(c => c.Notifications)
                .Returns(mockSet.Object);

            var service = new NotificationService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteNotificationAsync(notification);
            var deletedNotification = await mockContext.Object.Notifications.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedNotification);
        }
    }
}