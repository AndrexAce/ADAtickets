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
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Attachment"/> entities in the underlying database.
    /// </summary>
    internal sealed class AttachmentRepository(ADAticketsDbContext context) : IAttachmentRepository
    {
        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentByIdAsync"/>
        public async Task<Attachment?> GetAttachmentByIdAsync(Guid id)
        {
            return await context.Attachments.FindAsync(id);
        }

        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentsAsync"/>
        public async Task<IEnumerable<Attachment>> GetAttachmentsAsync()
        {
            return await context.Attachments.ToListAsync();
        }

        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentsByAsync"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The comparison with the StringComparison overload is not translatable by Entity Framework and the EF.Function.ILike method is not standard SQL but PostgreSQL dialect.")]
        public async Task<IEnumerable<Attachment>> GetAttachmentsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Attachment> query = context.Attachments;

            foreach (KeyValuePair<string, string> filter in filters)
            {
                switch (filter.Key.Pascalize())
                {
                    case nameof(Attachment.Id) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(attachment => attachment.Id == outGuid);
                        break;

                    case nameof(Attachment.TicketId) when Guid.TryParse(filter.Value, out Guid outGuid):
                        query = query.Where(attachment => attachment.TicketId == outGuid);
                        break;

                    case nameof(Attachment.Path):
                        query = query.Where(attachment => attachment.Path.ToLower().Contains(filter.Value.ToLower()));
                        break;

                    default:
                        return [];
                }
            }

            return await query.ToListAsync();
        }

        /// <inheritdoc cref="IAttachmentRepository.AddAttachmentAsync"/>
        public async Task AddAttachmentAsync(Attachment attachment, byte[] data)
        {
            string? fullPath = await SaveAttachmentToFileSystem(attachment.Path, data);

            if (fullPath is not null)
            {
                attachment.Path = fullPath;

                _ = context.Attachments.Add(attachment);
                _ = await context.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IAttachmentRepository.UpdateAttachmentAsync"/>
        public async Task UpdateAttachmentAsync(Attachment attachment, byte[] data, string oldAttachmentPath)
        {
            string? fullPath = await ReplaceAttachmentInFileSystem(attachment.Path, data, oldAttachmentPath);

            if (fullPath is not null)
            {
                attachment.Path = fullPath;

                _ = context.Attachments.Update(attachment);
                _ = await context.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IAttachmentRepository.DeleteAttachmentAsync"/>
        public async Task DeleteAttachmentAsync(Attachment attachment)
        {
            if (DeleteAttachmentFromFileSystem(attachment.Path))
            {
                _ = context.Attachments.Remove(attachment);
                _ = await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// <para>Stores an attachment file on the server's filesystem.</para>
        /// <para>If <paramref name="attachmentData"/> is empty, the method will save an empty file without launching exceptions.</para>
        /// </summary>
        /// <param name="attachmentName">The name of the attachment file.</param>
        /// <param name="attachmentData">Byte array encoding the file data.</param>
        /// <returns>A <see cref="Task"/> returning the full path if the attachment was successfully saved, or <see langword="null"/> otherwise.</returns>
        private static async Task<string?> SaveAttachmentToFileSystem(string attachmentName, byte[] attachmentData)
        {
            try
            {
                string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "media");
                string saveDirectoryPath = Path.Combine(baseDirectory, DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString());

                _ = Directory.CreateDirectory(saveDirectoryPath);

                string fullPath = Path.Combine(saveDirectoryPath, attachmentName);
                await File.WriteAllBytesAsync(fullPath, attachmentData);

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes an attachment file from the server's filesystem.
        /// </summary>
        /// <param name="attachmentPath">Full path of the attachment file.</param>
        /// <returns><see langword="true"/> if the attachment was successfully deleted, and <see langword="false"/> otherwise.</returns>
        private static bool DeleteAttachmentFromFileSystem(string attachmentPath)
        {
            try
            {
                if (!Path.IsPathRooted(attachmentPath))
                {
                    return false;
                }

                if (File.Exists(attachmentPath))
                {
                    File.Delete(attachmentPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// <para>Replaces an attachment file on the server's filesystem.</para>
        /// </summary>
        /// <param name="attachmentName">The new name of the attachment file.</param>
        /// <param name="attachmentData">Byte array encoding the file data.</param>
        /// <param name="oldAttachmentPath">Full path of the old attachment file.</param>
        /// <returns>A <see cref="Task"/> returning the full path if the attachment was successfully replaced, or <see langword="null"/> otherwise.</returns>
        private static async Task<string?> ReplaceAttachmentInFileSystem(string attachmentName, byte[] attachmentData, string oldAttachmentPath)
        {
            return DeleteAttachmentFromFileSystem(oldAttachmentPath) ? await SaveAttachmentToFileSystem(attachmentName, attachmentData) : null;
        }
    }
}
