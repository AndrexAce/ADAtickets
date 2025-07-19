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
using System.Text.RegularExpressions;

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
            if (await SaveAttachmentToFileSystem(attachment.Path, data))
            {
                // Since the path contains only the file name, make the attachment path match with the file system path.
                attachment.Path = Path.Combine("media/", DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString(), attachment.Path);

                _ = context.Attachments.Add(attachment);
                _ = await context.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IAttachmentRepository.UpdateAttachmentAsync"/>
        public async Task UpdateAttachmentAsync(Attachment attachment, byte[] data, string oldAttachmentPath)
        {
            if (await ReplaceAttachmentInFileSystem(attachment.Path, data, oldAttachmentPath))
            {
                // Since the path contains only the file name, make the attachment path match with the file system path.
                attachment.Path = Path.Combine("media/", DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString(), attachment.Path);

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
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if the attachment was successfully saved, and <see langword="false"/> otherwise.</returns>
        private static async Task<bool> SaveAttachmentToFileSystem(string attachmentName, byte[] attachmentData)
        {
            try
            {
                string saveDirectoryPath = Path.Combine("media/", DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString());

                _ = Directory.CreateDirectory(saveDirectoryPath);

                await File.WriteAllBytesAsync(Path.Combine(saveDirectoryPath, attachmentName), attachmentData);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes an attachment file from the server's filesystem.
        /// </summary>
        /// <param name="attachmentPath">Full path of the attachment file.</param>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if the attachment was successfully deleted, and <see langword="false"/> otherwise.</returns>
        private static bool DeleteAttachmentFromFileSystem(string attachmentPath)
        {
            try
            {
                if (!Regex.IsMatch(attachmentPath, @"^(?!.*//)[a-zA-Z0-9\-\\/\.]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                {
                    return false;
                }
                else if (File.Exists(attachmentPath))
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
        /// <returns></returns>
        private static async Task<bool> ReplaceAttachmentInFileSystem(string attachmentName, byte[] attachmentData, string oldAttachmentPath)
        {
            return DeleteAttachmentFromFileSystem(oldAttachmentPath) && await SaveAttachmentToFileSystem(attachmentName, attachmentData);
        }
    }
}
