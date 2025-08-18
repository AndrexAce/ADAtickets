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
///     Tests for <see cref="Controller.CreateOperatorEditNotificationsAsync(Ticket, Guid?, Guid)" />
///     <list type="number">
///         <item>Operator unassigned -> creates unassignment notifications</item>
///         <item>Operator assigned as first assignment -> creates assignment notifications</item>
///         <item>Operator assigned as reassignment from old operator -> creates assignment notifications</item>
///         <item>Operator unassigned but no operators in the system edge case -> creates unassignment notification only</item>
///         <item>Operator assigned as new operator but they are also creator edge case -> </item>
///         <item>Reassignment to the same operator edge case -> creates correct notifications</item>
///         <item>Operator assigned but old operator has invalid identifier edge case -> creates correct notifications</item>
///         <item>Ticket creator and editor are the same person edge case -> creates correct notifications</item>
///     </list>
/// </summary>
public sealed class CreateOperatorEditNotificationsAsyncTests
{
    private readonly Controller controller;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<INotificationRepository> mockNotificationRepository;
    private readonly Mock<IUserNotificationRepository> mockUserNotificationRepository;
    private readonly Mock<IUserPlatformRepository> mockUserPlatformRepository;
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<IHubContext<NotificationsHub>> mockHubContext;

    public CreateOperatorEditNotificationsAsyncTests()
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

    #region CreateOperatorEditNotificationsAsync Tests

    [Fact]
    public async Task CreateOperatorEditNotificationsAsyncOperatorUnassignedCreatesUnassignmentNotifications()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();
        var operatorUserId1 = Guid.NewGuid();
        var operatorUserId2 = Guid.NewGuid();
        var oldAssignedOperator = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = null, // Operator has been unassigned
            PlatformId = platformId
        };

        var operators = new List<User>
        {
            new() { Id = operatorUserId1, Type = UserType.Operator },
            new() { Id = operatorUserId2, Type = UserType.Admin },
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(operators);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Verify unassignment notification was created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketUnassigned &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator notification link was created
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify all operators were notified (2 operators: Admin + Operator types)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId1)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId2)), Times.Once);

        // Total: 1 creator + 2 operators = 3 user notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));

        // Verify only one notification was created (unassignment notification)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task CreateOperatorEditNotificationsAsyncOperatorAssignedFirstTimeCreatesAssignmentNotifications()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var newOperatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = newOperatorUserId, // Operator has been assigned
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = null; // First assignment scenario

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Verify "assigned to you" notification for new operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify "ticket assigned" notification for creator and old operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify new operator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        // Verify creator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator = 2 user notifications (no old operator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify exactly 2 notifications were created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateOperatorEditNotificationsAsyncOperatorReassignedCreatesAssignmentNotifications()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var newOperatorUserId = Guid.NewGuid();
        var oldOperatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = newOperatorUserId, // New operator assigned
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = oldOperatorUserId; // Reassignment scenario

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Verify "assigned to you" notification for new operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify "ticket assigned" notification for creator and old operator
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify new operator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        // Verify creator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify old operator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == oldOperatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator + 1 old operator = 3 user notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));

        // Verify exactly 2 notifications were created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task
        CreateOperatorEditNotificationsAsyncOperatorUnassignedWithNoOperatorsCreatesUnassignmentNotificationOnly()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = null, // Operator has been unassigned
            PlatformId = platformId
        };

        var users = new List<User>
        {
            new() { Id = creatorUserId, Type = UserType.User } // Only regular users, no operators/admins
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(users);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, Guid.NewGuid(), editorUserId);

        // Assert
        // Verify unassignment notification was created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketUnassigned &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator notification link was created
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // No operators to notify, so only 1 user notification (creator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task
        CreateOperatorEditNotificationsAsyncSamePersonAsCreatorAndNewOperatorCreatesCorrectNotifications()
    {
        // Arrange - Edge case where creator becomes the operator
        var creatorOperatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorOperatorUserId,
            OperatorUserId = creatorOperatorUserId, // Creator becomes operator
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = null; // First assignment

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should create notifications normally, even if creator and operator are the same person
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == creatorOperatorUserId)), Times.Once);

        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssigned &&
                n.UserId == creatorOperatorUserId)), Times.Once);

        // Should notify the same person twice (as operator and as creator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorOperatorUserId)), Times.Exactly(2));

        // Total: 2 notifications, 2 user notifications
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Exactly(2));
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task CreateOperatorEditNotificationsAsyncReassignmentToSameOperatorCreatesCorrectNotifications()
    {
        // Arrange - Edge case where old and new operator are the same
        var creatorUserId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = operatorUserId,
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = operatorUserId; // Same operator as current

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should still create all notifications, even if old and new operator are the same
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == operatorUserId)), Times.Once);

        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssigned &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify new operator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Exactly(2));

        // Verify creator notification link
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 2 operator notifications + 1 creator = 3 user notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task
        CreateOperatorEditNotificationsAsyncOperatorAssignedOldOperatorIsEmptyGuidCreatesCorrectNotifications()
    {
        // Arrange - Edge case with Guid.Empty as old operator
        var creatorUserId = Guid.NewGuid();
        var newOperatorUserId = Guid.NewGuid();
        var editorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = newOperatorUserId,
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = Guid.Empty; // Empty GUID should still have HasValue = true

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should notify the empty GUID as old operator (even though it's not a valid user)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == Guid.Empty)), Times.Once);

        // Should still notify new operator and creator
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator + 1 old operator (Guid.Empty) = 3 user notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task CreateOperatorEditNotificationsAsyncCreatorAndEditorSamePersonCreatesCorrectNotifications()
    {
        // Arrange - Creator is also the editor
        var creatorEditorUserId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorEditorUserId,
            OperatorUserId = operatorUserId,
            PlatformId = platformId
        };

        var editorUserId = creatorEditorUserId; // Editor is the creator
        Guid? oldAssignedOperator = null;

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should create notifications with editor as responsible (who is also the creator)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.UserId == operatorUserId)),
            Times.Exactly(2)); // Both notifications should have operator as responsible

        // Should notify operator and creator (same person gets notified as creator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorEditorUserId)), Times.Once);

        // Total: 2 user notifications
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));
    }

    #endregion
}