using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    public class Reply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset ReplyDateTime { get; set; } = DateTimeOffset.Now;
        [Column(TypeName = "varchar(5000)")]
        public string Message { get; set; } = string.Empty;

        [Column(TypeName = "varchar(254)")]
        public string AuthorUserEmail { get; set; } = string.Empty;
        public User AuthorUser { get; set; } = new User();

        public Guid TicketId { get; set; } = Guid.Empty;
        public Ticket Ticket { get; set; } = new Ticket();
    }
}
