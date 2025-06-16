using ADAtickets.Shared.Models;

namespace ADAtickets.ApiService.Tests.Services.EditRepository
{
    internal static class Utilities
    {
        public static Edit CreateEdit(
               string description = "",
               Guid ticketId = default,
               Guid userId = default)
        {
            return new Edit
            {
                Description = description,
                TicketId = ticketId,
                UserId = userId
            };
        }
    }
}
