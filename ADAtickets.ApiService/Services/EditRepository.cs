using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Edit"/> entities in the underlying database.
    /// </summary>
    class EditRepository(ADAticketsDbContext context) : IEditRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IEditRepository.GetEditByIdAsync(Guid)"/>
        public async Task<Edit> GetEditByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IEditRepository.GetEditsAsync"/>
        public async IAsyncEnumerable<Edit> GetEditsAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IEditRepository.AddEditAsync(Edit)"/>
        public async Task<bool> AddEditAsync(Edit edit)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IEditRepository.UpdateEditAsync(Edit)"/>
        public async Task<bool> UpdateEditAsync(Edit edit)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IEditRepository.DeleteEditAsync(Guid)"/>
        public async Task<bool> DeleteEditAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
