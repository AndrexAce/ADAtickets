using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Notification"/> entities in the underlying database.
    /// </summary>
    class NotificationRepository(ADAticketsDbContext context) : INotificationRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="INotificationRepository.GetNotificationByIdAsync(Guid)"/>
        public async Task<Notification> GetNotificationByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="INotificationRepository.GetNotificationsAsync"/>
        public async IAsyncEnumerable<Notification> GetNotificationsAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="INotificationRepository.AddNotificationAsync(Notification)"/>
        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="INotificationRepository.UpdateNotificationAsync(Notification)"/>
        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="INotificationRepository.DeleteNotificationAsync(Guid)"/>
        public async Task<bool> DeleteNotificationAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
