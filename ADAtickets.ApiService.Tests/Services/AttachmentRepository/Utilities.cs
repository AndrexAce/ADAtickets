using ADAtickets.Shared.Models;

namespace ADAtickets.ApiService.Tests.Services.AttachmentRepository
{
    internal static class Utilities
    {
        public static Attachment CreateAttachment(
        string path = "",
        Guid ticketId = default)
        {
            return new Attachment
            {
                Path = path,
                TicketId = ticketId,
            };
        }
    }
}
