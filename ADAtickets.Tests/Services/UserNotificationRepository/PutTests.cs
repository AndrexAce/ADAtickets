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
using UserNotificationService = ADAtickets.ApiService.Services.UserNotificationRepository;

namespace ADAtickets.Tests.Services.UserNotificationRepository;

/// <summary>
///     <c>UpdateUserNotification(UserNotification)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public sealed class PutTests
{
    public static TheoryData<UserNotification> InvalidUserNotificationData =>
    [
        // Entity with keys referring to no existing userNotification or notification
        Utilities.CreateUserNotification(Guid.NewGuid(), Guid.NewGuid()),
        // Entity already existing in the relation
        Utilities.CreateUserNotification(Guid.Empty, Guid.Empty)
    ];

    public static TheoryData<UserNotification> ValidUserNotificationData =>
    [
        Utilities.CreateUserNotification(Guid.NewGuid(), Guid.NewGuid())
    ];

    [Theory]
    [MemberData(nameof(ValidUserNotificationData))]
    public async Task UpdateUserNotification_ValidEntity_ReturnsNew(UserNotification inUserNotification)
    {
        // Arrange
        List<User> users = [new() { Id = inUserNotification.ReceiverUserId }];
        List<Notification> notifications =
            [new() { Id = Guid.Empty }, new() { Id = inUserNotification.NotificationId }];
        List<UserNotification> userNotifications =
        [
            new()
            {
                Id = inUserNotification.Id, ReceiverUserId = inUserNotification.ReceiverUserId,
                NotificationId = Guid.Empty
            }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserNotification>> mockUserNotificationSet = userNotifications.BuildMockDbSet();
        _ = mockUserNotificationSet.Setup(s => s.Update(It.IsAny<UserNotification>()))
            .Callback<UserNotification>(un =>
            {
                if (users.Find(u => u.Id == un.ReceiverUserId) is not null
                    && notifications.Find(p => p.Id == un.NotificationId) is not null
                    && userNotifications.Find(u =>
                        u.ReceiverUserId == un.ReceiverUserId && u.NotificationId == un.NotificationId) is null)
                {
                    userNotifications[0].ReceiverUserId = inUserNotification.ReceiverUserId;
                    userNotifications[0].NotificationId = inUserNotification.NotificationId;
                }
            });
        _ = mockContext.Setup(c => c.UserNotifications)
            .Returns(mockUserNotificationSet.Object);

        UserNotificationService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserNotificationAsync(inUserNotification);
        var updatedUserNotification =
            await mockContext.Object.UserNotifications.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUserNotification);
        Assert.True(inUserNotification.ReceiverUserId == updatedUserNotification.ReceiverUserId &&
                    inUserNotification.NotificationId == updatedUserNotification.NotificationId);
    }

    [Theory]
    [MemberData(nameof(InvalidUserNotificationData))]
    public async Task UpdateUserNotification_InvalidEntity_ReturnsOld(UserNotification inUserNotification)
    {
        // Arrange
        List<User> users = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];
        List<Notification> notifications = [new() { Id = Guid.AllBitsSet }, new() { Id = Guid.Empty }];
        List<UserNotification> userNotifications =
        [
            new() { Id = inUserNotification.Id, ReceiverUserId = Guid.AllBitsSet, NotificationId = Guid.AllBitsSet },
            new() { Id = Guid.NewGuid(), ReceiverUserId = Guid.Empty, NotificationId = Guid.Empty }
        ];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<UserNotification>> mockUserNotificationSet = userNotifications.BuildMockDbSet();
        _ = mockUserNotificationSet.Setup(s => s.Update(It.IsAny<UserNotification>()))
            .Callback<UserNotification>(un =>
            {
                if (users.Find(u => u.Id == un.ReceiverUserId) is not null
                    && notifications.Find(p => p.Id == un.NotificationId) is not null
                    && userNotifications.Find(u =>
                        u.ReceiverUserId == un.ReceiverUserId && u.NotificationId == un.NotificationId) is null)
                {
                    userNotifications[0].ReceiverUserId = inUserNotification.ReceiverUserId;
                    userNotifications[0].NotificationId = inUserNotification.NotificationId;
                }
            });
        _ = mockContext.Setup(c => c.UserNotifications)
            .Returns(mockUserNotificationSet.Object);

        UserNotificationService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateUserNotificationAsync(inUserNotification);
        var updatedUserNotification = await mockContext.Object.UserNotifications.FirstOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedUserNotification);
        Assert.True(inUserNotification.ReceiverUserId != updatedUserNotification.ReceiverUserId ||
                    inUserNotification.NotificationId != updatedUserNotification.NotificationId);
    }
}