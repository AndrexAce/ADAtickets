using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Reply"/> entities in the underlying database.
    /// </summary>
    class ReplyRepository(ADAticketsDbContext context) : IReplyRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="IReplyRepository.GetReplyByIdAsync(Guid)"/>
        public Task<Reply> GetReplyByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IReplyRepository.GetRepliesAsync"/>
        public IAsyncEnumerable<Reply> GetRepliesAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IReplyRepository.AddReplyAsync(Reply)"/>
        public Task<bool> AddReplyAsync(Reply reply)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IReplyRepository.UpdateReplyAsync(Reply)"/>
        public Task<bool> UpdateReplyAsync(Reply reply)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="IReplyRepository.DeleteReplyAsync(Guid)"/>
        public Task<bool> DeleteReplyAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
