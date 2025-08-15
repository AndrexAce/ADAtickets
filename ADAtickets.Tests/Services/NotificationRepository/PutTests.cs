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

namespace ADAtickets.Tests.Services.NotificationRepository;

/// <summary>
///     <c>UpdateNotification(Notification)</c>
///     <list type="number">
///         <item>Valid entity</item>
///         <item>Invalid entity</item>
///     </list>
/// </summary>
public class PutTests
{
    public static TheoryData<Notification> InvalidNotificationData =>
    [
        Utilities.CreateNotification(new string('a', 201), Guid.AllBitsSet, Guid.AllBitsSet),
        Utilities.CreateNotification("Valid message.", Guid.Empty, Guid.AllBitsSet),
        Utilities.CreateNotification("Valid message.", Guid.AllBitsSet, Guid.Empty)
    ];

    public static TheoryData<Notification> ValidNotificationData =>
    [
        Utilities.CreateNotification("Valid message.", Guid.AllBitsSet, Guid.AllBitsSet)
    ];

    [Theory]
    [MemberData(nameof(ValidNotificationData))]
    public async Task UpdateNotification_ValidEntity_ReturnsNew(Notification inNotification)
    {
        // Arrange
        List<Notification> notifications =
        [
            new()
            {
                Id = inNotification.Id, Message = "Old message.", TicketId = Guid.AllBitsSet, UserId = Guid.AllBitsSet
            }
        ];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Notification>> mockNotificationSet = notifications.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockNotificationSet.Setup(s => s.Update(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                if (n.Message.Length <= 200 && mockTicketSet.Object.Single().Id == n.TicketId &&
                    mockUserSet.Object.Single().Id == n.UserId) notifications[0].Message = inNotification.Message;
            });
        _ = mockContext.Setup(c => c.Notifications)
            .Returns(mockNotificationSet.Object);

        NotificationService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateNotificationAsync(inNotification);
        var updatedNotification = await mockContext.Object.Notifications.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedNotification);
        Assert.Equal(inNotification.Message, updatedNotification.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidNotificationData))]
    public async Task UpdateNotification_InvalidEntity_ReturnsOld(Notification inNotification)
    {
        // Arrange
        List<Notification> notifications =
        [
            new()
            {
                Id = inNotification.Id, Message = "Old message.", TicketId = Guid.AllBitsSet, UserId = Guid.AllBitsSet
            }
        ];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];
        List<User> users = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Notification>> mockNotificationSet = notifications.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        Mock<DbSet<User>> mockUserSet = users.BuildMockDbSet();
        _ = mockNotificationSet.Setup(s => s.Update(It.IsAny<Notification>()))
            .Callback<Notification>(n =>
            {
                if (n.Message.Length <= 200 && mockTicketSet.Object.Single().Id == n.TicketId &&
                    mockUserSet.Object.Single().Id == n.UserId) notifications[0].Message = inNotification.Message;
            });
        _ = mockContext.Setup(c => c.Notifications)
            .Returns(mockNotificationSet.Object);

        NotificationService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.UpdateNotificationAsync(inNotification);
        var updatedNotification = await mockContext.Object.Notifications.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(updatedNotification);
        Assert.NotEqual(inNotification.Message, updatedNotification.Message);
    }
}