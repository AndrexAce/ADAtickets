using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;

namespace ADAtickets.ApiService.Services
{
    /// <summary>
    /// Implements the methods to manage the <see cref="Ticket"/> entities in the underlying database.
    /// </summary>
    class TicketRepository(ADAticketsDbContext context) : ITicketRepository
    {
        readonly ADAticketsDbContext _context = context;

        /// <inheritdoc cref="ITicketRepository.GetTicketByIdAsync(Guid)"/>
        public async Task<Ticket> GetTicketByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="ITicketRepository.GetTicketsAsync"/>
        public async IAsyncEnumerable<Ticket> GetTicketsAsync()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="ITicketRepository.GetTicketsByUserIdAsync(Guid)"/>
        public async Task<bool> AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="ITicketRepository.UpdateTicketAsync(Ticket)"/>
        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc cref="ITicketRepository.DeleteTicketAsync(Guid)"/>
        public async Task<bool> DeleteTicketAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
