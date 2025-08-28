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
///     Tests for <see cref="Controller.CreateEditNotificationsAsync(Ticket, Guid)" />.
///     This method handles ticket edit notifications with the following logic:
///     <list type="number">
///         <item>Always creates a "TicketEdited" notification with the editor as responsible</item>
///         <item>Determines notification recipients based on editor identity using three-way conditional:
///             <list type="bullet">
///                 <item>If editor == ticket.CreatorUserId: notify assigned operator OR all operators if none assigned</item>
///                 <item>Else if editor == ticket.OperatorUserId: notify creator only</item>
///                 <item>Else (third party): notify creator AND (assigned operator OR all operators if none assigned)</item>
///             </list>
///         </item>
///         <item>SendNotificationToAllOperators is called when ticket.OperatorUserId is null or empty</item>
///         <item>SendNotificationToAllOperators filters users by Type == Admin || Type == Operator</item>
///     </list>
///     Test scenarios covered:
///     <list type="number">
///         <item>Editor is creator with assigned operator -> notifies assigned operator only</item>
///         <item>Editor is creator without assigned operator -> notifies all Admin/Operator users</item>
///         <item>Editor is assigned operator -> notifies creator only</item>
///         <item>Editor is third party with assigned operator -> notifies creator AND assigned operator</item>
///         <item>Editor is third party without assigned operator -> notifies creator AND all Admin/Operator users</item>
///         <item>Edge case: creator and operator are same person -> follows creator branch (notifies self)</item>
///         <item>Edge case: operator editing when creator is different -> follows operator branch</item>
///         <item>Edge case: no Admin/Operator users exist -> SendNotificationToAllOperators finds no targets</item>
///     </list>
/// </summary>
public sealed class CreateEditNotificationsAsyncTests
{
    private readonly Controller controller;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<INotificationRepository> mockNotificationRepository;
    private readonly Mock<IUserNotificationRepository> mockUserNotificationRepository;
    private readonly Mock<IUserPlatformRepository> mockUserPlatformRepository;
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<IHubContext<NotificationsHub>> mockHubContext;

    public CreateEditNotificationsAsyncTests()
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

    #region CreateEditNotificationsAsync Tests

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsCreatorWithAssignedOperatorNotifiesOperator()
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
            OperatorUserId = operatorUserId, // Has assigned operator
            PlatformId = platformId
        };

        var editorUserId = creatorUserId; // Editor matches creator - first branch condition

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify assigned operator was notified (creator branch + HasValue condition)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

        // Verify only one notification created and one UserNotification entry
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsCreatorWithoutAssignedOperatorNotifiesAllOperators()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = null, // No assigned operator triggers SendNotificationToAllOperators
            PlatformId = platformId
        };

        var editorUserId = creatorUserId; // Editor matches creator - first branch condition

        var allOperators = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.Operator },
            new() { Id = Guid.NewGuid(), Type = UserType.Admin },
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified by SendNotificationToAllOperators
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allOperators);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called - should notify Admin + Operator types (2 notifications)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify only one notification was created (the TicketEdited notification)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsAssignedOperatorNotifiesCreator()
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
            OperatorUserId = operatorUserId,
            PlatformId = platformId
        };

        var editorUserId = operatorUserId; // Editor matches assigned operator - second branch condition

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator was notified (operator branch - simple creator notification)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify only one notification created and one UserNotification entry
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsThirdPartyWithAssignedOperatorNotifiesCreatorAndOperator()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid(); // Third party editor (admin)
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = operatorUserId, // Has assigned operator
            PlatformId = platformId
        };

        var editorUserId = adminUserId; // Editor matches neither creator nor operator - third branch (else)

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator was notified (third party branch - always notifies creator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify assigned operator was notified (third party branch + HasValue condition)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

        // Verify one notification created and two UserNotification entries
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task
        CreateEditNotificationsAsyncEditorIsThirdPartyWithoutAssignedOperatorNotifiesCreatorAndAllOperators()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid(); // Third party editor (admin)
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = null, // No assigned operator triggers SendNotificationToAllOperators
            PlatformId = platformId
        };

        var editorUserId = adminUserId; // Editor matches neither creator nor operator - third branch (else)

        var allOperators = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.Operator },
            new() { Id = Guid.NewGuid(), Type = UserType.Admin },
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified by SendNotificationToAllOperators
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allOperators);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify creator was notified (third party branch - always notifies creator)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called - should notify Admin + Operator types (2 additional notifications)
        // Total: 1 creator + 2 operators = 3 UserNotification entries
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(3));

        // Verify only one notification was created (the TicketEdited notification)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsCreatorAndOperatorSamePersonNotifiesAllOperators()
    {
        // Arrange - Edge case where creator and operator are the same person
        var creatorOperatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorOperatorUserId,
            OperatorUserId = creatorOperatorUserId, // Same person is creator and operator
            PlatformId = platformId
        };

        var editorUserId = creatorOperatorUserId; // Editor is creator (first condition takes precedence)

        var allOperators = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.Operator },
            new() { Id = Guid.NewGuid(), Type = UserType.Admin }
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(allOperators);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Since editor == ticket.CreatorUserId (first condition), follow creator branch
        // and notify the assigned operator (which is themselves) - HasValue is true
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorOperatorUserId)), Times.Once);

        // Verify only one notification created and one UserNotification entry
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncEditorIsOperatorWhenCreatorIsOperatorNotifiesCreator()
    {
        // Arrange - Edge case where editor is the operator, but creator is also an operator (different person)
        var creatorUserId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = operatorUserId,
            PlatformId = platformId
        };

        var editorUserId = operatorUserId; // Editor matches assigned operator - second branch condition

        // Setup mocks
        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Since editor == ticket.OperatorUserId (second condition), follow operator branch
        // and notify the creator only
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Verify only one notification created and one UserNotification entry
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateEditNotificationsAsyncNoOperatorsInSystemCreatesNotificationWithoutOperatorNotifications()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketId,
            CreatorUserId = creatorUserId,
            OperatorUserId = null, // No assigned operator triggers SendNotificationToAllOperators
            PlatformId = platformId
        };

        var editorUserId = creatorUserId; // Editor matches creator - first branch condition

        var onlyRegularUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.User },
            new() { Id = creatorUserId, Type = UserType.User }
        };

        // Setup mocks
        mockUserRepository.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(onlyRegularUsers);

        mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await controller.CreateEditNotificationsAsync(ticket, editorUserId);

        // Assert
        // Verify TicketEdited notification was created with editor as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketEdited &&
                n.UserId == editorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called but found no Admin/Operator users to notify
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Never);

        // Verify only one notification was created (the TicketEdited notification)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
    }

    #endregion
}