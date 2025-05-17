using ADAtickets.ApiService.Models;

namespace ADAtickets.ApiService.Tests.Services.TicketRepository
{
    internal static class Utilities
    {
        public static Ticket CreateTicket(
               string title = "",
               string description = "",
               Guid platformId = default,
               Guid creatorUserId = default,
               Guid? operatorUserId = default)
        {
            return new Ticket
            {
                Title = title,
                Description = description,
                PlatformId = platformId,
                CreatorUserId = creatorUserId,
                OperatorUserId = operatorUserId
            };
        }
    }
}
