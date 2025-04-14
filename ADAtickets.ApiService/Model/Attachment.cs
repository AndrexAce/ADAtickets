using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    [PrimaryKey(nameof(Path), nameof(TicketId))]
    public class Attachment
    {
        [Column(TypeName = "varchar(4000)")]
        public string Path { get; set; } = string.Empty;

        public Guid TicketId { get; set; } = Guid.Empty;
        public Ticket Ticket { get; set; } = new Ticket();
    }
}
