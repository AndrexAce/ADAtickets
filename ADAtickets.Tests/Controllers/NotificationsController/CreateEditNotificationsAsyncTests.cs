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
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Models;
using AutoMapper;
using Moq;
using Controller = ADAtickets.ApiService.Controllers.NotificationsController;

namespace ADAtickets.Tests.Controllers.NotificationsController
{
    /// <summary>
    /// Tests for <see cref="Controller.CreateEditNotificationsAsync(Ticket, Guid)"/>.
    /// <list type="number">
    ///     <item>Editor is ticket creator with assigned operator -> notifies operator</item>
    ///     <item>Editor is ticket creator without assigned operator -> notifies all operators</item>
    ///     <item>Editor is assigned operator -> notifies creator</item>
    ///     <item>Editor is third party with assigned operator -> notifies creator and operator</item>
    ///     <item>Editor is third party without assigned operator -> notifies creator and all operators</item>
    ///     <item>Ticket editor and creator are the same person edge case -> notifies all operators</item>
    ///     <item>Editor is ticket operator and creator is an operator edge case -> notifies creator</item>
    ///     <item>No operators in the system -> creates notification but not operator notification links</item>
    /// </list>
    /// </summary>
    public sealed class CreateEditNotificationsAsyncTests
    {
        private readonly Mock<INotificationRepository> _mockNotificationRepository;
        private readonly Mock<IUserNotificationRepository> _mockUserNotificationRepository;
        private readonly Mock<IUserPlatformRepository> _mockUserPlatformRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Controller _controller;

        public CreateEditNotificationsAsyncTests()
        {
            _mockNotificationRepository = new Mock<INotificationRepository>();
            _mockUserNotificationRepository = new Mock<IUserNotificationRepository>();
            _mockUserPlatformRepository = new Mock<IUserPlatformRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();

            _controller = new Controller(
                _mockNotificationRepository.Object,
                _mockMapper.Object,
                _mockUserNotificationRepository.Object,
                _mockUserPlatformRepository.Object,
                _mockUserRepository.Object);
        }

        #region CreateEditNotificationsAsync Tests

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsCreatorWithAssignedOperator_NotifiesOperator()
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

            var editorUserId = creatorUserId; // Editor is the creator

            // Setup mocks
            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId &&
                    !n.IsRead)), Times.Once);

            // Verify assigned operator was notified
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

            // Verify only one notification created and one user notification
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsCreatorWithoutAssignedOperator_NotifiesAllOperators()
        {
            // Arrange
            var creatorUserId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var platformId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                CreatorUserId = creatorUserId,
                OperatorUserId = null, // No assigned operator
                PlatformId = platformId
            };

            var editorUserId = creatorUserId; // Editor is the creator

            var allOperators = new List<User>
            {
                new() { Id = Guid.NewGuid(), Type = UserType.Operator },
                new() { Id = Guid.NewGuid(), Type = UserType.Admin },
                new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified
            };

            // Setup mocks
            _mockUserRepository.Setup(x => x.GetUsersAsync())
                .ReturnsAsync(allOperators);

            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId)), Times.Once);

            // Verify all operators (Admin + Operator types) were notified - should be 2 notifications
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Exactly(2));

            // Verify only one notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsAssignedOperator_NotifiesCreator()
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

            var editorUserId = operatorUserId; // Editor is the assigned operator

            // Setup mocks
            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId)), Times.Once);

            // Verify creator was notified
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

            // Verify only one notification created and one user notification
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsThirdPartyWithAssignedOperator_NotifiesCreatorAndOperator()
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

            var editorUserId = adminUserId; // Editor is neither creator nor assigned operator

            // Setup mocks
            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId)), Times.Once);

            // Verify creator was notified
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

            // Verify assigned operator was notified
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Once);

            // Verify one notification created and two user notifications
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsThirdPartyWithoutAssignedOperator_NotifiesCreatorAndAllOperators()
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
                OperatorUserId = null, // No assigned operator
                PlatformId = platformId
            };

            var editorUserId = adminUserId; // Editor is neither creator nor assigned operator

            var allOperators = new List<User>
            {
                new() { Id = Guid.NewGuid(), Type = UserType.Operator },
                new() { Id = Guid.NewGuid(), Type = UserType.Admin },
                new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified
            };

            // Setup mocks
            _mockUserRepository.Setup(x => x.GetUsersAsync())
                .ReturnsAsync(allOperators);

            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId)), Times.Once);

            // Verify creator was notified
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

            // Verify all operators (Admin + Operator types) were notified - should be 2 additional notifications
            // Total: 1 creator + 2 operators = 3 user notifications
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Exactly(3));

            // Verify only one notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsCreatorAndOperatorSamePerson_NotifiesAllOperators()
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

            var editorUserId = creatorOperatorUserId; // Editor is creator (and also operator)

            var allOperators = new List<User>
            {
                new() { Id = Guid.NewGuid(), Type = UserType.Operator },
                new() { Id = Guid.NewGuid(), Type = UserType.Admin }
            };

            // Setup mocks
            _mockUserRepository.Setup(x => x.GetUsersAsync())
                .ReturnsAsync(allOperators);

            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Since editor == ticket.CreatorUserId, it should follow the first branch
            // and notify the assigned operator (which is themselves), so it should notify themselves
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == creatorOperatorUserId)), Times.Once);

            // Verify one notification created and one user notification
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_EditorIsOperatorWhenCreatorIsOperator_NotifiesCreator()
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

            var editorUserId = operatorUserId; // Editor is the assigned operator

            // Setup mocks
            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Since editor == ticket.OperatorUserId, it should follow the second branch
            // and notify the creator
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
                It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

            // Verify one notification created and one user notification
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Once);
        }

        [Fact]
        public async Task CreateEditNotificationsAsync_NoOperatorsInSystem_CreatesNotificationWithoutOperatorNotifications()
        {
            // Arrange
            var creatorUserId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var platformId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                CreatorUserId = creatorUserId,
                OperatorUserId = null, // No assigned operator
                PlatformId = platformId
            };

            var editorUserId = creatorUserId; // Editor is the creator

            var onlyRegularUsers = new List<User>
            {
                new() { Id = Guid.NewGuid(), Type = UserType.User },
                new() { Id = creatorUserId, Type = UserType.User }
            };

            // Setup mocks
            _mockUserRepository.Setup(x => x.GetUsersAsync())
                .ReturnsAsync(onlyRegularUsers);

            _mockNotificationRepository.Setup(x => x.AddNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _mockUserNotificationRepository.Setup(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.CreateEditNotificationsAsync(ticket, editorUserId);

            // Assert
            // Verify ticket edited notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(
                It.Is<Notification>(n =>
                    n.TicketId == ticketId &&
                    n.Message == Notifications.TicketEdited &&
                    n.UserId == editorUserId)), Times.Once);

            // Verify no user notifications were created (no operators to notify)
            _mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()), Times.Never);

            // Verify only one notification was created
            _mockNotificationRepository.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        #endregion
    }
}