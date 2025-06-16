using ADAtickets.Shared.Models;

namespace ADAtickets.ApiService.Tests.Services.ReplyRepository
{
    internal static class Utilities
    {
        public static Reply CreateReply(
               string message = "",
               Guid authorUserId = default,
               Guid ticketId = default)
        {
            return new Reply
            {
                Message = message,
                AuthorUserId = authorUserId,
                TicketId = ticketId
            };
        }
    }
}
