using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SendDateTime { get; } = DateTimeOffset.Now;
        [Column(TypeName = "varchar(200)")]
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        public Guid TicketId { get; set; } = Guid.Empty;
        public Ticket Ticket { get; set; } = new Ticket();

        [Column(TypeName = "varchar(254)")]
        public string UserEmail { get; set; } = string.Empty;
        public User User { get; set; } = new User();

        public ICollection<UserNotification> SentNotifications { get; } = [];
    }
}
