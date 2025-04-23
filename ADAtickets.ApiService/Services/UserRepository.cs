using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="User"/> entities in the underlying database.
    /// </summary>
    class UserRepository(ADAticketsDbContext context) : IUserRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IUserRepository.GetUserByEmailAsync(string)"/>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IUserRepository.GetUsersAsync"/>
        public async IAsyncEnumerable<User> GetUsersAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IUserRepository.AddUserAsync(User)"/>
        public async Task<bool> AddUserAsync(User user)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IUserRepository.UpdateUserAsync(User)"/>
        public async Task<bool> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IUserRepository.DeleteUserAsync(string)"/>
        public async Task<bool> DeleteUserAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
