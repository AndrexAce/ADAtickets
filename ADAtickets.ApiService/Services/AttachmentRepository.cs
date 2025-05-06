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
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Attachment"/> entities in the underlying database.
    /// </summary>
    sealed class AttachmentRepository(ADAticketsDbContext context) : IAttachmentRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentByIdAsync"/>
        public async Task<Attachment?> GetAttachmentByIdAsync(Guid id)
        {
            return await _context.Attachments.FindAsync(id);
        }

        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentsAsync"/>
        public async Task<IEnumerable<Attachment>> GetAttachmentsAsync()
        {
            return await _context.Attachments.ToListAsync();
        }

        /// <inheritdoc cref="IAttachmentRepository.GetAttachmentsByAsync"/>
        public async Task<IEnumerable<Attachment>> GetAttachmentsByAsync(IEnumerable<KeyValuePair<string, string>> filters)
        {
            IQueryable<Attachment> query = _context.Attachments;

            foreach (var filter in filters)
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
                        query = query.Where(attachment => attachment.Path.Contains(filter.Value, StringComparison.InvariantCultureIgnoreCase));
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
                attachment.Path = Path.Combine("media/", attachment.TicketId.ToString(), attachment.Path);

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IAttachmentRepository.UpdateAttachmentAsync"/>
        public async Task UpdateAttachmentAsync(Attachment attachment, byte[] data, string oldAttachmentPath)
        {
            if (await ReplaceAttachmentInFileSystem(attachment.Path, data, oldAttachmentPath))
            {
                // Since the path contains only the file name, make the attachment path match with the file system path.
                attachment.Path = Path.Combine("media/", attachment.TicketId.ToString(), attachment.Path);

                _context.Attachments.Update(attachment);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IAttachmentRepository.DeleteAttachmentAsync"/>
        public async Task DeleteAttachmentAsync(Attachment attachment)
        {
            if (DeleteAttachmentFromFileSystem(attachment.Path))
            {
                _context.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        private async static Task<bool> SaveAttachmentToFileSystem(string attachmentPath, byte[] attachmentData)
        {
            try
            {
                var saveDirectoryPath = Path.Combine("media/", DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString());

                Directory.CreateDirectory(saveDirectoryPath);

                await File.WriteAllBytesAsync(Path.Combine(saveDirectoryPath, attachmentPath), attachmentData);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool DeleteAttachmentFromFileSystem(string attachmentPath)
        {
            try
            {
                if (File.Exists(attachmentPath))
                {
                    File.Delete(attachmentPath);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static async Task<bool> ReplaceAttachmentInFileSystem(string attachmentPath, byte[] attachmentData, string oldAttachmentPath)
        {
            if (DeleteAttachmentFromFileSystem(oldAttachmentPath))
            {
                return await SaveAttachmentToFileSystem(attachmentPath, attachmentData);
            }

            return false;
        }
    }
}
