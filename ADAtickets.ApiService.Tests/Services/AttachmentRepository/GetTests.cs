﻿/*
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
using MockQueryable.Moq;
using Moq;
using AttachmentService = ADAtickets.ApiService.Services.AttachmentRepository;

namespace ADAtickets.ApiService.Tests.Services.AttachmentRepository
{
    /// <summary>
    /// <c>GetAttachmentByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing id</item>
    ///     <item>Non-existing id</item>
    ///     <item>Empty id</item>   
    /// </list>
    /// <c>GetAttachmentsAsync()</c>
    /// <list type="number">
    ///     <item>Empty set</item>
    ///     <item>Full set</item>
    /// </list>
    /// </summary>
    public sealed class GetTests
    {
        #region GetOne
        [Fact]
        public async Task GetAttachmentByIdAsync_ExistingId_ReturnsAttachment()
        {
            // Arrange
            Guid existingId = Guid.NewGuid();

            List<Attachment> attachments = [new() { Id = existingId }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => attachments.Find(a => a.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            Attachment? result = await service.GetAttachmentByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
        }

        [Fact]
        public async Task GetAttachmentByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => attachments.Find(a => a.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            Attachment? result = await service.GetAttachmentByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAttachmentByIdAsync_EmptyId_ReturnsNull()
        {
            // Arrange
            List<Attachment> attachments = [new() { Id = Guid.NewGuid() }];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockSet.Setup(s => s.FindAsync(It.IsAny<Guid>()))
                .ReturnsAsync((object[] arguments) => attachments.Find(a => a.Id == (Guid)arguments[0]));
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            Attachment? result = await service.GetAttachmentByIdAsync(Guid.Empty);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetAttachments_EmptySet_ReturnsNothing()
        {
            // Arrange
            List<Attachment> attachments = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachments_FullSet_ReturnsAttachments()
        {
            // Arrange
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            List<Attachment> attachments =
            [
                new() { Id = guid1 },
                new() { Id = guid2 },
                new() { Id = guid3 }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(guid1, result.ElementAt(0).Id);
            Assert.Equal(guid2, result.ElementAt(1).Id);
            Assert.Equal(guid3, result.ElementAt(2).Id);
        }
        #endregion

        #region GetBy
        [Fact]
        public async Task GetAttachmentsBy_OneFilterWithMatch_ReturnsAttachments()
        {
            // Arrange
            List<Attachment> attachments =
            [
                new() { Path = "/path/example.png" },
                new() { Path = "/path/trial.png"},
                new() { Path = "/path/test.png" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsByAsync([new KeyValuePair<string, string>("Path", "path")]);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains("path", result.ElementAt(0).Path, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("path", result.ElementAt(1).Path, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("path", result.ElementAt(2).Path, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task GetAttachmentsBy_MoreFiltersWithMatch_ReturnAttachments()
        {
            // Arrange
            List<Attachment> attachments =
            [
                new() { Path = "/path/example.png", TicketId = Guid.AllBitsSet },
                new() { Path = "/path/trial.png" },
                new() { Path = "/path/test.png", TicketId = Guid.AllBitsSet }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsByAsync([
                new KeyValuePair<string, string>("Path", "path"),
                new KeyValuePair<string, string>("TicketId", Guid.AllBitsSet.ToString())
                ]);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("path", result.ElementAt(0).Path, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("path", result.ElementAt(1).Path, StringComparison.InvariantCultureIgnoreCase);
            Assert.Equal(Guid.AllBitsSet, result.ElementAt(0).TicketId);
            Assert.Equal(Guid.AllBitsSet, result.ElementAt(1).TicketId);
        }

        [Fact]
        public async Task GetAttachmentsBy_NoMatch_ReturnsNothing()
        {
            // Arrange
            List<Attachment> attachments =
            [
                new() { Path = "/path/example.png" },
                new() { Path = "/path/trial.png" },
                new() { Path = "/path/test.png" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsByAsync([new KeyValuePair<string, string>("Path", "image")]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttachmentsBy_InvalidFilter_ReturnsNothing()
        {
            // Arrange
            List<Attachment> attachments =
            [
                new() { Path = "/path/example.png" },
                new() { Path = "/path/trial.png" },
                new() { Path = "/path/test.png" }
            ];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<Microsoft.EntityFrameworkCore.DbSet<Attachment>> mockSet = attachments.BuildMockDbSet();
            _ = mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            AttachmentService service = new(mockContext.Object);

            // Act
            IEnumerable<Attachment> result = await service.GetAttachmentsByAsync([new KeyValuePair<string, string>("SomeName", "value")]);

            // Assert
            Assert.Empty(result);
        }
        #endregion
    }
}