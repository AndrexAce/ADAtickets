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
///     This method handles ticket creation notifications with the following logic:
///     <list type="number">
///         <item>Always creates an initial "TicketCreated" notification with the creator as responsible</item>
///         <item>Searches for UserPlatform entries matching the ticket's platform ID</item>
///         <item>If platform preferences exist:
///             <list type="bullet">
///                 <item>Performs LINQ join between UserPlatform entries and all users by UserId</item>
///                 <item>Orders joined results by user.AssignedTickets.Count ascending</item>
///                 <item>Selects the first operator ID (least workload, or first if tied)</item>
///                 <item>If valid operator found (non-Guid.Empty): auto-assigns with 3 notifications total</item>
///                 <item>If no valid operator (Guid.Empty): falls back to SendNotificationToAllOperators</item>
///             </list>
///         </item>
///         <item>If no platform preferences exist: calls SendNotificationToAllOperators immediately</item>
///         <item>SendNotificationToAllOperators notifies all users with Type == Admin || Type == Operator</item>
///         <item>Returns assigned operator's ID on successful auto-assignment, null otherwise</item>
///     </list>
///     Test scenarios covered:
///     <list type="number">
///         <item>No UserPlatform entries for ticket's platform -> SendNotificationToAllOperators called</item>
///         <item>UserPlatform entries exist but LINQ join yields no matches -> SendNotificationToAllOperators called</item>
///         <item>Valid operator found via join -> auto-assigns with full notification chain (3 notifications)</item>
///         <item>Multiple operators with different AssignedTickets.Count -> selects minimum count</item>
///         <item>Multiple operators with same AssignedTickets.Count -> FirstOrDefault behavior</item>
///         <item>No Admin/Operator users exist -> SendNotificationToAllOperators finds no targets</item>
///         <item>LINQ join succeeds but FirstOrDefault returns Guid.Empty -> SendNotificationToAllOperators fallback</item>
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
            new() { Id = Guid.NewGuid(), Type = UserType.User } // Should not be notified by SendNotificationToAllOperators
        };

        // No UserPlatform entries for this platform - triggers immediate SendNotificationToAllOperators call
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
        Assert.Null(result); // Should return null when SendNotificationToAllOperators path is taken

        // Verify initial TicketCreated notification with creator as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called - should notify Admin + Operator types (2 notifications)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify only the initial notification was created (no auto-assignment notifications)
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
            new() { UserId = Guid.NewGuid(), PlatformId = platformId } // UserPlatform exists but...
        };

        var allUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Type = UserType.User }, // Different UserId - LINQ join will fail
            new() { Id = Guid.NewGuid(), Type = UserType.Operator } // Different UserId - LINQ join will fail
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
        Assert.Null(result); // Should return null when LINQ join yields no results and falls back to SendNotificationToAllOperators

        // Verify initial TicketCreated notification with creator as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called - but only 1 operator in allUsers matches Admin/Operator filter
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(1));

        // Verify only the initial notification was created (no auto-assignment occurred)
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
            new() { UserId = operatorUserId, PlatformId = platformId } // UserPlatform exists and...
        };

        var allUsers = new List<User>
        {
            new() { Id = operatorUserId, Type = UserType.Operator } // Matching UserId - LINQ join succeeds, AssignedTickets.Count = 0
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
        Assert.Equal(operatorUserId, result); // Should return the auto-assigned operator's ID

        // Verify initial TicketCreated notification with creator as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify TicketAssignedToYouBySystem notification with operator as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssignedToYouBySystem &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify TicketAssigned notification with operator as responsible (for creator)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketAssigned &&
                n.UserId == operatorUserId)), Times.Once);

        // Verify UserNotification entries: operator gets 2 (creation + assignment), creator gets 1 (assignment)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == operatorUserId)), Times.Exactly(2));

        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(
            It.Is<UserNotification>(un => un.ReceiverUserId == creatorUserId)), Times.Once);

        // Total: 3 notifications created, 3 UserNotification entries created
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
            new() { Id = operator1Id, Type = UserType.Operator }, // Will have 2 AssignedTickets.Count
            new() { Id = operator2Id, Type = UserType.Operator }, // Will have 0 AssignedTickets.Count - should be selected by orderby
            new() { Id = operator3Id, Type = UserType.Operator } // Will have 1 AssignedTickets.Count
        };

        // Simulate different workloads by adding tickets to AssignedTickets collections
        allUsers[0].AssignedTickets.Add(new Ticket());
        allUsers[0].AssignedTickets.Add(new Ticket());
        // operator2 has 0 tickets (default)
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
        Assert.Equal(operator2Id, result); // Should return operator2 (AssignedTickets.Count = 0, minimum)

        // Verify auto-assignment notifications were created for the operator with least workload
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
            new() { Id = operator1Id, Type = UserType.Operator }, // Will have 1 AssignedTickets.Count
            new() { Id = operator2Id, Type = UserType.Operator } // Will have 1 AssignedTickets.Count (same as operator1)
        };
        // Simulate same workload - both operators have 1 assigned ticket
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
        Assert.Equal(operator1Id, result); // Should return operator1 (FirstOrDefault when orderby results in tie)

        // Verify auto-assignment notifications were created for the first operator in the ordered sequence
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
            new() { Id = Guid.NewGuid(), Type = UserType.User }, // Regular users won't be notified by SendNotificationToAllOperators
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
        Assert.Null(result); // Should return null when SendNotificationToAllOperators path is taken

        // Verify only the initial TicketCreated notification with creator as responsible
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.TicketId == ticketId &&
                n.Message == Notifications.TicketCreated &&
                n.UserId == creatorUserId)), Times.Once);

        // Verify SendNotificationToAllOperators was called but found no Admin/Operator users to notify
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Never);

        // Verify only one notification was created (no auto-assignment, no operator notifications)
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
            new() { UserId = Guid.Empty, PlatformId = platformId } // UserPlatform with invalid UserId
        };

        var allUsers = new List<User>
        {
            new() { Id = Guid.Empty, Type = UserType.Operator }, // LINQ join will succeed but FirstOrDefault returns Guid.Empty
            new() { Id = Guid.NewGuid(), Type = UserType.Operator } // This will be notified by SendNotificationToAllOperators
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
        Assert.Null(result); // Should return null when operatorWithLeastWorkload == Guid.Empty triggers fallback

        // Verify initial TicketCreated notification was created
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketCreated)), Times.Once);

        // Verify SendNotificationToAllOperators was called - should notify both Operator users (including Guid.Empty)
        mockUserNotificationRepository.Verify(x => x.AddUserNotificationAsync(It.IsAny<UserNotification>()),
            Times.Exactly(2));

        // Verify no auto-assignment notifications were created (fallback path taken)
        mockNotificationRepository.Verify(x => x.AddNotificationAsync(
            It.Is<Notification>(n =>
                n.Message == Notifications.TicketAssignedToYouBySystem)), Times.Never);
    }

    #endregion
}