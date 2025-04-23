using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Platform"/> entities in the underlying database.
    /// </summary>
    class PlatformRepository(ADAticketsDbContext context) : IPlatformRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IPlatformRepository.GetPlatformByNameAsync(string)"/>
        public async Task<Platform> GetPlatformByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IPlatformRepository.GetPlatformsAsync"/>
        public async IAsyncEnumerable<Platform> GetPlatformsAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IPlatformRepository.AddPlatformAsync(Platform)"/>
        public async Task<bool> AddPlatformAsync(Platform platform)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IPlatformRepository.UpdatePlatformAsync(Platform)"/>
        public async Task<bool> UpdatePlatformAsync(Platform platform)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IPlatformRepository.DeletePlatformAsync(string)"/>
        public async Task<bool> DeletePlatformAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
