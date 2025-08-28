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
///     This method handles operator assignment/unassignment notifications with the following logic:
///     <list type="number">
///         <item>Binary decision based on ticket.OperatorUserId state:
///             <list type="bullet">
///                 <item>If ticket.OperatorUserId is null: UNASSIGNMENT path</item>
///                 <item>If ticket.OperatorUserId has value: ASSIGNMENT path</item>
///             </list>
///         </item>
///         <item>UNASSIGNMENT path (ticket.OperatorUserId is null):
///             <list type="bullet">
///                 <item>Creates "TicketUnassigned" notification with editor as responsible</item>
///                 <item>Notifies creator via UserNotification</item>
///                 <item>Calls SendNotificationToAllOperators (filters by Type == Admin || Type == Operator)</item>
///                 <item>Total: 1 notification, 1+ UserNotification entries (creator + all operators)</item>
///             </list>
///         </item>
///         <item>ASSIGNMENT path (ticket.OperatorUserId has value):
///             <list type="bullet">
///                 <item>Creates "TicketAssignedToYou" notification with new operator as responsible</item>
///                 <item>Notifies new operator via UserNotification</item>
///                 <item>Creates "TicketAssigned" notification with new operator as responsible</item>
///                 <item>Notifies creator via UserNotification</item>
///                 <item>If oldAssignedOperator.HasValue: also notifies old operator via UserNotification</item>
///                 <item>Total: 2 notifications, 2-3 UserNotification entries (new operator + creator + optional old operator)</item>
///             </list>
///         </item>
///         <item>The method does NOT validate user relationships or prevent duplicate notifications</item>
///     </list>
///     Test scenarios covered:
///     <list type="number">
///         <item>Unassignment path: ticket.OperatorUserId == null -> TicketUnassigned + SendNotificationToAllOperators</item>
///         <item>Assignment path (first time): ticket.OperatorUserId != null, oldAssignedOperator == null -> 2 notifications, 2 UserNotifications</item>
///         <item>Assignment path (reassignment): ticket.OperatorUserId != null, oldAssignedOperator != null -> 2 notifications, 3 UserNotifications</item>
///         <item>Edge case: SendNotificationToAllOperators with no Admin/Operator users -> minimal UserNotifications</item>
///         <item>Edge case: Creator and new operator are same person -> creates duplicate notifications to same user</item>
///         <item>Edge case: Old and new operator are same person -> creates notifications as if different people</item>
///         <item>Edge case: oldAssignedOperator == Guid.Empty but HasValue == true -> still notifies Guid.Empty</item>
///         <item>Edge case: Creator is also editor -> notifications still use correct responsible user IDs</item>
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
            OperatorUserId = null, // Triggers UNASSIGNMENT path (ticket.OperatorUserId is null)
            PlatformId = platformId
        };

        var operators = new List<User>
        {
            new() { Id = operatorUserId1, Type = UserType.Operator },
            new() { Id = operatorUserId2, Type = UserType.Admin },
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified by SendNotificationToAllOperators
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
        // Verify TicketUnassigned notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketUnassigned &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator was notified (always happens in unassignment path)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called - should notify Admin + Operator types
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId1)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId2)), Times.Once);

        // Total: 1 creator + 2 operators = 3 UserNotification entries
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));

        // Verify only one notification was created (unassignment notification only)
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
            OperatorUserId = newOperatorUserId, // Triggers ASSIGNMENT path (ticket.OperatorUserId has value)
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = null; // First assignment scenario (oldAssignedOperator.HasValue == false)

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Verify TicketAssignedToYou notification for new operator (first notification in assignment path)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify TicketAssigned notification for creator and old operator (second notification in assignment path)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify new operator UserNotification link (from first notification)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        // Verify creator UserNotification link (from second notification)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator = 2 UserNotification entries (no old operator since oldAssignedOperator is null)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify exactly 2 notifications were created (assignment path creates 2 notifications)
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
            OperatorUserId = newOperatorUserId, // Triggers ASSIGNMENT path (ticket.OperatorUserId has value)
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = oldOperatorUserId; // Reassignment scenario (oldAssignedOperator.HasValue == true)

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Verify TicketAssignedToYou notification for new operator (first notification in assignment path)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify TicketAssigned notification for creator and old operator (second notification in assignment path)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == newOperatorUserId)), Times.Once);

        // Verify new operator UserNotification link (from first notification)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        // Verify creator UserNotification link (from second notification)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify old operator UserNotification link (from second notification, oldAssignedOperator.HasValue condition)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == oldOperatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator + 1 old operator = 3 UserNotification entries
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));

        // Verify exactly 2 notifications were created (assignment path always creates 2 notifications)
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
            OperatorUserId = null, // Triggers UNASSIGNMENT path (ticket.OperatorUserId is null)
            PlatformId = platformId
        };

        var users = new List<User>
        {
            new() { Id = creatorUserId, Type = UserType.User } // Only regular users, no Admin/Operator types for SendNotificationToAllOperators
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
        // Verify TicketUnassigned notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketUnassigned &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator was notified (always happens in unassignment path)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // SendNotificationToAllOperators was called but found no Admin/Operator users to notify
        // Only 1 UserNotification entry (creator only)
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
            OperatorUserId = creatorOperatorUserId, // Triggers ASSIGNMENT path, creator and operator are same person
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = null; // First assignment (oldAssignedOperator.HasValue == false)

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

        // Should notify the same person twice (as new operator and as creator) - method doesn't prevent duplicates
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorOperatorUserId)), Times.Exactly(2));

        // Total: 2 notifications (assignment path), 2 UserNotification entries (both to same person)
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
            OperatorUserId = operatorUserId, // Triggers ASSIGNMENT path
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = operatorUserId; // Same operator as current (oldAssignedOperator.HasValue == true)

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should still create all notifications, even if old and new operator are the same - method doesn't validate this
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYou &&
                n.UserId == operatorUserId)), Times.Once);

        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssigned &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify operator gets notified twice (as new operator and as old operator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Exactly(2));

        // Verify creator gets notified once
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 2 operator notifications + 1 creator = 3 UserNotification entries
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
            OperatorUserId = newOperatorUserId, // Triggers ASSIGNMENT path
            PlatformId = platformId
        };

        Guid? oldAssignedOperator = Guid.Empty; // Empty GUID but HasValue == true, so oldAssignedOperator condition will execute

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should notify the empty GUID as old operator (even though it's not a valid user) - method doesn't validate user IDs
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == Guid.Empty)), Times.Once);

        // Should still notify new operator and creator normally
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == newOperatorUserId)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 1 new operator + 1 creator + 1 old operator (Guid.Empty) = 3 UserNotification entries
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
            OperatorUserId = operatorUserId, // Triggers ASSIGNMENT path
            PlatformId = platformId
        };

        var editorUserId = creatorEditorUserId; // Editor is the creator
        Guid? oldAssignedOperator = null; // First assignment (oldAssignedOperator.HasValue == false)

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateOperatorEditNotificationsAsync(ticket, oldAssignedOperator, editorUserId);

        // Assert
        // Should create notifications with new operator as responsible (assignment path uses ticket.OperatorUserId.Value)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.UserId == operatorUserId)), // Both notifications in assignment path use new operator as responsible
            Times.Exactly(2));

        // Should notify operator and creator (who is also the editor)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorEditorUserId)), Times.Once);

        // Total: 2 UserNotification entries (1 operator + 1 creator/editor)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));
    }

    #endregion
}