﻿/*
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
using NotificationService = ADAtickets.ApiService.Services.NotificationRepository;

namespace ADAtickets.ApiService.Tests.Services.NotificationRepository
{
    /// <summary>
    /// <c>AddNotificationAsync(Notification)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public partial class PostTests
    {
        public static TheoryData<Notification> InvalidNotificationData =>
        [
            Utilities.CreateNotification(message: new string('a', 201), ticketId: Guid.AllBitsSet, userId: Guid.AllBitsSet),
            Utilities.CreateNotification(message: "Valid message.", ticketId: Guid.Empty, userId : Guid.AllBitsSet),
            Utilities.CreateNotification(message: "Valid message.", ticketId: Guid.AllBitsSet, userId: Guid.Empty),
        ];

        public static TheoryData<Notification> ValidNotificationData =>
        [
            Utilities.CreateNotification(message: "Valid message.", ticketId: Guid.AllBitsSet, userId: Guid.AllBitsSet)
        ];

        [Theory]
        [MemberData(nameof(ValidNotificationData))]
        public async Task AddNotification_ValidEntity_ReturnsNotification(Notification inNotification)
        {
            // Arrange
            var notifications = new List<Notification>();
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockNotificationSet = notifications.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockNotificationSet.Setup(s => s.Add(It.IsAny<Notification>()))
                .Callback<Notification>(n =>
                {
                    if (n.Message.Length <= 200 && mockTicketSet.Object.Single().Id == n.TicketId && mockUserSet.Object.Single().Id == n.UserId)
                    {
                        notifications.Add(n);
                    }
                });
            mockContext.Setup(c => c.Notifications)
                .Returns(mockNotificationSet.Object);

            var service = new NotificationService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddNotificationAsync(inNotification);
            var addedNotification = await mockContext.Object.Notifications.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(addedNotification);
            Assert.NotEmpty(notifications);
        }

        [Theory]
        [MemberData(nameof(InvalidNotificationData))]
        public async Task AddNotification_InvalidEntity_ReturnsNothing(Notification inNotification)
        {
            // Arrange
            var notifications = new List<Notification>();
            var tickets = new List<Ticket> { new() { Id = Guid.AllBitsSet } };
            var users = new List<User> { new() { Id = Guid.AllBitsSet } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockNotificationSet = notifications.BuildMockDbSet();
            var mockTicketSet = tickets.BuildMockDbSet();
            var mockUserSet = users.BuildMockDbSet();
            mockNotificationSet.Setup(s => s.Add(It.IsAny<Notification>()))
                .Callback<Notification>(n =>
                {
                    if (n.Message.Length <= 200 && mockTicketSet.Object.Single().Id == n.TicketId && mockUserSet.Object.Single().Id == n.UserId)
                    {
                        notifications.Add(n);
                    }
                });
            mockContext.Setup(c => c.Notifications)
                .Returns(mockNotificationSet.Object);

            var service = new NotificationService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddNotificationAsync(inNotification);
            var addedNotification = await mockContext.Object.Notifications.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(addedNotification);
            Assert.Empty(notifications);
        }
    }
}
