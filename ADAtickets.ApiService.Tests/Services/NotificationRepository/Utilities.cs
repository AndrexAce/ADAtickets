using ADAtickets.ApiService.Models;

namespace ADAtickets.ApiService.Tests.Services.NotificationRepository
{
    internal static class Utilities
    {
        public static Notification CreateNotification(
               string message = "",
               Guid ticketId = default,
               Guid userId = default)
        {
            return new Notification
            {
                Message = message,
                TicketId = ticketId,
                UserId = userId
            };
        }
    }
}
