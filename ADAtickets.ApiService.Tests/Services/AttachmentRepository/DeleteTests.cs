/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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

namespace ADAtickets.ApiService.Tests.Services.AttachmentRepository
{
    /// <summary>
    /// <c>DeleteAttachmentByIdAsync(Guid)</c>
    /// <list type="number">
    ///     <item>Existing entity</item>
    /// </list>
    /// </summary>
    sealed public class DeleteTests
    {
        [Fact]
        public async Task DeleteAttachmentByIdAsync_ExistingEntity_DeletesEntity()
        {
            // Arrange
            var attachment = new Attachment { Id = Guid.NewGuid(), Path = "delete.png" };
            var attachments = new List<Attachment> { attachment };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockSet = attachments.BuildMockDbSet();
            mockSet.Setup(s => s.Remove(It.IsAny<Attachment>()))
                .Callback<Attachment>(attachment => attachments.RemoveAll(a => a.Id == attachment.Id));
            mockContext.Setup(c => c.Attachments)
                .Returns(mockSet.Object);

            var service = new AttachmentService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.DeleteAttachmentAsync(attachment);
            var deletedAttachment = await mockContext.Object.Attachments.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(deletedAttachment);
            Assert.False(File.Exists(attachment.Path));
        }
    }
}