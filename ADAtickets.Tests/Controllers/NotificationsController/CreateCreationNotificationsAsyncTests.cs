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

using ADAtickets.ApiService.Hubs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Models;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Controller = ADAtickets.ApiService.Controllers.NotificationsController;

namespace ADAtickets.Tests.Controllers.NotificationsController;

/// <summary>
///     Tests for <see cref="Controller.CreateCreationNotificationsAsync(Ticket)" />.
///     <list type="number">
///         <item>No operators with preferred platform -> notifies all operators</item>
///         <item>Operators with preferred platform but no valid users -> notifies all operators</item>
///         <item>Operators with preferred platform and valid user with least workload -> auto-assigns</item>
///         <item>Multiple operators with different workloads -> selects least workload</item>
///         <item>Operators with same workload -> selects first one</item>
///         <item>No operators in system edge case -> creates only initial notification</item>
///         <item>Operator with empty Guid ID edge case -> notifies all operators</item>
///     </list>
/// </summary>
public sealed class CreateCreationNotificationsAsyncTests
{
    private readonly Controller controller;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<INotificationRepository> mockNotificationRepository;
    private readonly Mock<IUserNotificationRepository> mockUserNotificationRepository;
    private readonly Mock<IUserPlatformRepository> mockUserPlatformRepository;
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<IHubContext<NotificationsHub>> mockHubContext;

    public CreateCreationNotificationsAsyncTests()
    {
        mockNotificationRepository = new Mock<INotificationRepository>();
        mockUserNotificationRepository = new Mock<IUserNotificationRepository>();
        mockUserPlatformRepository = new Mock<IUserPlatformRepository>();
        mockUserRepository = new Mock<IUserRepository>();
        mockMapper = new Mock<IMapper>();
        mockHubContext = new Mock<IHubContext<NotificationsHub>>();

        controller = new Controller(
            mockNotificationRepository.Object,
            mockMapper.Object,
            mockUserNotificationRepository.Object,
            mockUserPlatformRepository.Object,
            mockUserRepository.Object,
            mockHubContext.Object);
    }

    #region CreateCreationNotificationsAsync Tests

    [Fact]
    public async Task CreateCreationNotificationsAsyncNoOperatorsWithPreferredPlatformNotifiesAllOperators()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var allOperators = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.Operator },
            new() { Id = Guid.NewGuid(), Type = UserType.Admin },
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified
        };

        // No operators with preferred platform
        var emptyUserPlatforms = new List<UserPlatform>();

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(emptyUserPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allOperators);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Null(result); // Should return null when no auto-assignment occurs

        // Verify initial ticket created notification
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify all operators (Admin + Operator types) were notified - should be 2 notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify no additional notifications were created (only the initial one)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task
        CreateCreationNotificationsAsyncOperatorsWithPreferredPlatformButNoValidUsersNotifiesAllOperators()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var userPlatforms = new List<UserPlatform>
        {
            new() { UserId = Guid.NewGuid(), PlatformId = platformId }
        };

        var allUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.User }, // Different user, won't match join
            new() { Id = Guid.NewGuid(), Type = UserType.Operator }
        };

        var allOperators = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.Operator },
            new() { Id = Guid.NewGuid(), Type = UserType.Admin }
        };

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(userPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Null(result); // Should return null when no valid operator found

        // Verify initial ticket created notification
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called (operators from second call to GetUsersAsync)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(1)); // Only operators from allUsers

        // Verify only the initial notification was created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task CreateCreationNotificationsAsyncOperatorWithPreferredPlatformAndLeastWorkloadAutoAssignsTicket()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var userPlatforms = new List<UserPlatform>
        {
            new() { UserId = operatorUserId, PlatformId = platformId }
        };

        var allUsers = new List<User>
        {
            new() { Id = operatorUserId, Type = UserType.Operator } // Empty workload
        };

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(userPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Equal(operatorUserId, result); // Should return the assigned operator's ID

        // Verify initial ticket created notification
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify system assignment notification for operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYouBySystem &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify ticket assigned notification for creator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify user notifications were created (operator about creation + operator about assignment + creator about assignment)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Exactly(2));

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 3 notifications, 3 user notification links
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Exactly(3));
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task CreateCreationNotificationsAsyncMultipleOperatorsWithDifferentWorkloadsSelectsLeastWorkload()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var operator1Id = Guid.NewGuid();
        var operator2Id = Guid.NewGuid();
        var operator3Id = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var userPlatforms = new List<UserPlatform>
        {
            new() { UserId = operator1Id, PlatformId = platformId },
            new() { UserId = operator2Id, PlatformId = platformId },
            new() { UserId = operator3Id, PlatformId = platformId }
        };

        var allUsers = new List<User>
        {
            new() { Id = operator1Id, Type = UserType.Operator }, // 2 tickets
            new() { Id = operator2Id, Type = UserType.Operator }, // 0 tickets - should be selected
            new() { Id = operator3Id, Type = UserType.Operator } // 1 ticket
        };

        // Add 2 tickets to operator1
        allUsers[0].AssignedTickets.Add(new Ticket());
        allUsers[0].AssignedTickets.Add(new Ticket());
        // Add 1 ticket to operator3
        allUsers[2].AssignedTickets.Add(new Ticket());

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(userPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Equal(operator2Id, result); // Should return operator2 (least workload)

        // Verify notifications were created for the correct operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYouBySystem &&
                n.UserId == operator2Id)), Times.Once);

        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssigned &&
                n.UserId == operator2Id)), Times.Once);
    }

    [Fact]
    public async Task CreateCreationNotificationsAsyncOperatorsWithSameWorkloadSelectsFirstOne()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var operator1Id = Guid.NewGuid();
        var operator2Id = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var userPlatforms = new List<UserPlatform>
        {
            new() { UserId = operator1Id, PlatformId = platformId },
            new() { UserId = operator2Id, PlatformId = platformId }
        };

        var allUsers = new List<User>
        {
            new() { Id = operator1Id, Type = UserType.Operator }, // 1 ticket
            new() { Id = operator2Id, Type = UserType.Operator } // 1 ticket (same workload)
        };
        // Add 1 ticket to both operators to simulate same workload
        allUsers[0].AssignedTickets.Add(new Ticket());
        allUsers[1].AssignedTickets.Add(new Ticket());

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(userPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Equal(operator1Id, result); // Should return the first operator (FirstOrDefault behavior)

        // Verify notifications were created for the first operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYouBySystem &&
                n.UserId == operator1Id)), Times.Once);
    }

    [Fact]
    public async Task CreateCreationNotificationsAsyncNoOperatorsInSystemCreatesOnlyInitialNotification()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var emptyUserPlatforms = new List<UserPlatform>();
        var onlyRegularUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.User }, // Only regular users, no operators
            new() { Id = creatorUserId, Type = UserType.User }
        };

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(emptyUserPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(onlyRegularUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Null(result); // Should return null when no operators exist

        // Verify only the initial ticket created notification
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify no user notifications were created (no operators to notify)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Never);

        // Verify only one notification was created total
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task CreateCreationNotificationsAsyncOperatorWithEmptyGuidIdNotifiesAllOperators()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            PlatformId = platformId
        };

        var userPlatforms = new List<UserPlatform>
        {
            new() { UserId = Guid.Empty, PlatformId = platformId } // Invalid user ID
        };

        var allUsers = new List<User>
        {
            new() { Id = Guid.Empty, Type = UserType.Operator }, // Should be skipped due to Guid.Empty
            new() { Id = Guid.NewGuid(), Type = UserType.Operator }
        };

        // Setup mocks
        mockUserPlatformRepository.Setup(x => x.GetUserPlatformsByAsync(
                It.Is<IEnumerable<KeyValuePair<string, string>>>(filters =>
                    filters.Any(f => f.Key == nameof(UserPlatform.PlatformId) && f.Value == platformId.ToString()))))
            .ReturnsAsync(userPlatforms);

        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCreationNotificationsAsync(ticket);

        // Assert
        Assert.Null(result); // Should return null when operatorWithLeastWorkload is Guid.Empty

        // Verify initial notification was created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketCreated)), Times.Once);

        // Verify SendNotificationToAllOperators was called (should notify all the operators)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify no auto-assignment notifications were created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYouBySystem)), Times.Never);
    }

    #endregion
}