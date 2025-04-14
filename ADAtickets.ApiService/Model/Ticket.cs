using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TicketType Type { get; set; } = TicketType.BUG;
        public DateTimeOffset CreationDateTime { get; } = DateTimeOffset.Now;
        [Column(TypeName = "varchar(50)")]
        public string Title { get; set; } = string.Empty;
        [Column(TypeName = "varchar(5000)")]
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.LOW;
        public Status Status { get; set; } = Status.UNASSIGNED;
        public int WorkItemId { get; set; } = 0;

        [Column(TypeName = "varchar(50)")]
        public string PlatformName { get; set; } = string.Empty;
        public Platform Platform { get; set; } = new Platform();

        [ForeignKey(nameof(CreatorUser))]
        [Column(TypeName = "varchar(254)")]
        public string CreatorUserEmail { get; set; } = string.Empty;
        public User CreatorUser { get; set; } = new User();

        [ForeignKey(nameof(OperatorUser))]
        [Column(TypeName = "varchar(254)")]
        public string OperatorUserEmail { get; set; } = string.Empty;
        public User OperatorUser { get; set; } = new User();

        public ICollection<Edit> Edits { get; } = [];

        public ICollection<Reply> Replies { get; } = [];

        public ICollection<Attachment> Attachments { get; } = [];

        public ICollection<Notification> Notifications { get; } = [];
    }
}
