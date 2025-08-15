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

namespace ADAtickets.Tests.Services.AttachmentRepository
{
    /// <summary>
    /// <c>UpdateAttachment(Attachment, byte[], string)</c>
    /// <list type="number">
    ///     <item>Valid new entity, empty data, valid old path</item>
    ///     <item>Valid new entity, valid data, valid old path</item>
    ///     <item>Invalid new entity, valid data, valid old path</item>
    ///     <item>Valid new entity, valid data, invalid old path</item>
    /// </list>
    /// </summary>
    public class PutTests
    {
        public static TheoryData<Attachment> InvalidAttachmentData =>
        [
            Utilities.CreateAttachment(path: "/" + new string('a', 3996) + ".png", ticketId: Guid.AllBitsSet),
            Utilities.CreateAttachment(path: "home/invalid", ticketId: Guid.AllBitsSet),
            Utilities.CreateAttachment(path: "valid.png", ticketId: Guid.Empty),
        ];

        public static TheoryData<Attachment> ValidAttachmentData =>
        [
            Utilities.CreateAttachment(path: "valid2.png", ticketId: Guid.AllBitsSet)
        ];

        [Theory]
        [MemberData(nameof(ValidAttachmentData))]
        public async Task UpdateAttachment_ValidNewEntityEmptyDataValidOldPath_ReturnsNew(Attachment inAttachment)
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = inAttachment.Id, Path = "/old.png", TicketId = Guid.AllBitsSet }];
            List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
            Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
            _ = mockAttachmentSet.Setup(s => s.Update(It.IsAny<Attachment>()))
                .Callback<Attachment>(a =>
                {
                    if (a.Id == attachments[0].Id && a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) && mockTicketSet.Object.Single().Id == a.TicketId)
                    {
                        attachments[0].Path = a.Path;
                    }
                });
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockAttachmentSet.Object);

            AttachmentService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateAttachmentAsync(inAttachment, [], attachments[0].Path);
            Attachment? updatedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedAttachment);
            Assert.Equal(inAttachment.Path, updatedAttachment.Path);
            Assert.Null(Record.Exception(() => File.Delete(updatedAttachment.Path)));
        }

        [Theory]
        [MemberData(nameof(ValidAttachmentData))]
        public async Task UpdateAttachment_ValidNewEntityValidDataValidOldPath_ReturnsNew(Attachment inAttachment)
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = inAttachment.Id, Path = "/old.png", TicketId = Guid.AllBitsSet }];
            List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
            Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
            _ = mockAttachmentSet.Setup(s => s.Update(It.IsAny<Attachment>()))
                .Callback<Attachment>(a =>
                {
                    if (a.Id == attachments[0].Id && a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) && mockTicketSet.Object.Single().Id == a.TicketId)
                    {
                        attachments[0].Path = a.Path;
                    }
                });
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockAttachmentSet.Object);

            AttachmentService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateAttachmentAsync(inAttachment, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], attachments[0].Path);
            Attachment? updatedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedAttachment);
            Assert.Equal(inAttachment.Path, updatedAttachment.Path);
            Assert.Null(Record.Exception(() => File.Delete(updatedAttachment.Path)));
        }

        [Theory]
        [MemberData(nameof(InvalidAttachmentData))]
        public async Task UpdateAttachment_InvalidNewEntityValidDataValidOldPath_ReturnsOld(Attachment inAttachment)
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = inAttachment.Id, Path = "/old.png", TicketId = Guid.AllBitsSet }];
            List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
            Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
            _ = mockAttachmentSet.Setup(s => s.Update(It.IsAny<Attachment>()))
                .Callback<Attachment>(a =>
                {
                    if (a.Id == attachments[0].Id && a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) && mockTicketSet.Object.Single().Id == a.TicketId)
                    {
                        attachments[0].Path = a.Path;
                    }
                });
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockAttachmentSet.Object);

            AttachmentService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateAttachmentAsync(inAttachment, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], attachments[0].Path);
            Attachment? updatedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedAttachment);
            Assert.NotEqual(inAttachment.Path, updatedAttachment.Path);
        }

        [Theory]
        [MemberData(nameof(ValidAttachmentData))]
        public async Task UpdateAttachment_ValidNewEntityValidDataInvalidOldPath_ReturnsOld(Attachment inAttachment)
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = inAttachment.Id, Path = "/old.png", TicketId = Guid.AllBitsSet }];
            List<Ticket> tickets = [new() { Id = Guid.AllBitsSet }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Attachment>> mockAttachmentSet = attachments.BuildMockDbSet();
            Mock<DbSet<Ticket>> mockTicketSet = tickets.BuildMockDbSet();
            _ = mockAttachmentSet.Setup(s => s.Update(It.IsAny<Attachment>()))
                .Callback<Attachment>(a =>
                {
                    if (a.Id == attachments[0].Id && a.Path.Length <= 4000 && Path.IsPathRooted(a.Path) && mockTicketSet.Object.Single().Id == a.TicketId)
                    {
                        attachments[0].Path = a.Path;
                    }
                });
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockAttachmentSet.Object);

            AttachmentService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdateAttachmentAsync(inAttachment, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "home" + attachments[0].Path);
            Attachment? updatedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedAttachment);
            Assert.NotEqual(inAttachment.Path, updatedAttachment.Path);
        }
    }
}
