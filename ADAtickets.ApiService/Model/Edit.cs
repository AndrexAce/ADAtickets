using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    public class Edit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset EditDateTime { get; set; } = DateTimeOffset.UtcNow;
        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; } = string.Empty;
        public Status OldStatus { get; set; } = Status.UNASSIGNED;
        public Status NewStatus { get; set; } = Status.UNASSIGNED;

        public Guid TicketId { get; set; } = Guid.Empty;
        public Ticket Ticket { get; set; } = new Ticket();

        [Column(TypeName = "varchar(254)")]
        public string UserEmail { get; set; } = string.Empty;
        public User User { get; set; } = new User();
    }
}
