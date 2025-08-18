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
using AttachmentService = ADAtickets.ApiService.Services.AttachmentRepository;

namespace ADAtickets.Tests.Services.AttachmentRepository;

/// <summary>
///     <c>AddAttachmentAsync(Attachment, byte[])</c>
///     <list type="number">
///         <item>Valid entity, empty data</item>
///         <item>Valid entity, valid data</item>
///         <item>Invalid entity, valid data</item>
///     </list>
/// </summary>
public sealed class PostTests
{
    public static TheoryData<Attachment> InvalidAttachmentData =>
    [
        Utilities.CreateAttachment("/" + new string('a', 3996) + ".png", Guid.AllBitsSet),
        Utilities.CreateAttachment("home/invalid", Guid.AllBitsSet),
        Utilities.CreateAttachment("valid.png", Guid.Empty)
    ];

    public static TheoryData<Attachment> ValidAttachmentData =>
    [
        Utilities.CreateAttachment("valid1.png", Guid.AllBitsSet)
    ];

    [Theory]
    [MemberData(nameof(ValidAttachmentData))]
    public async Task AddAttachment_ValidEntityEmptyData_ReturnsAttachment(Attachment inAttachment)
    {
        // Arrange
        List<Attachment> attachments = [];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        _ = mockAttachmentSet.Setup(s => s.Add(It.IsAny<Attachment>()))
            .Callback<Attachment>(a =>
            {
                if (a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) &&
                    mockTicketSet.Object.Single().Id == a.TicketId) attachments.Add(a);
            });
        _ = mockContext.Setup(c => c.Attachments)
            .Returns(mockAttachmentSet.Object);

        AttachmentService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddAttachmentAsync(inAttachment, []);
        var addedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(addedAttachment);
        Assert.NotEmpty(attachments);
        Assert.Null(Record.Exception(() => File.Delete(addedAttachment.Path)));
    }

    [Theory]
    [MemberData(nameof(ValidAttachmentData))]
    public async Task AddAttachment_ValidEntityValidData_ReturnsAttachment(Attachment inAttachment)
    {
        // Arrange
        List<Attachment> attachments = [];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        _ = mockAttachmentSet.Setup(s => s.Add(It.IsAny<Attachment>()))
            .Callback<Attachment>(a =>
            {
                if (a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) &&
                    mockTicketSet.Object.Single().Id == a.TicketId) attachments.Add(a);
            });
        _ = mockContext.Setup(c => c.Attachments)
            .Returns(mockAttachmentSet.Object);

        AttachmentService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddAttachmentAsync(inAttachment, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var addedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.NotNull(addedAttachment);
        Assert.NotEmpty(attachments);
        Assert.Null(Record.Exception(() => File.Delete(addedAttachment.Path)));
    }

    [Theory]
    [MemberData(nameof(InvalidAttachmentData))]
    public async Task AddAttachment_InvalidEntityValidData_ReturnsNothing(Attachment inAttachment)
    {
        // Arrange
        List<Attachment> attachments = [];
        List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

        Mock<ADAticketsDbContext> mockContext = new();
        Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
        Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
        _ = mockAttachmentSet.Setup(s => s.Add(It.IsAny<Attachment>()))
            .Callback<Attachment>(a =>
            {
                if (a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) &&
                    mockTicketSet.Object.Single().Id == a.TicketId) attachments.Add(a);
            });
        _ = mockContext.Setup(c => c.Attachments)
            .Returns(mockAttachmentSet.Object);

        AttachmentService service = new(mockContext.Object);

        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await service.AddAttachmentAsync(inAttachment, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var addedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

        // Assert
        Assert.Null(addedAttachment);
        Assert.Empty(attachments);
    }
}