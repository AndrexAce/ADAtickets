using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    public class User
    {
        [Key]
        [Column(TypeName = "varchar(254)")]
        public string Email { get; set; } = string.Empty;
        [Column(TypeName = "varchar(128)")]
        public string Password { get; set; } = string.Empty;
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "varchar(50)")]
        public string Surname { get; set; } = string.Empty;
        [Column(TypeName = "varchar(20)")]
        public string PhoneNumber { get; set; } = string.Empty;
        public bool Email2FA { get; set; } = false;
        public bool Phone2FA { get; set; } = false;
        public bool EmailNotifications { get; set; } = false;
        public bool PhoneNotifications { get; set; } = false;
        public UserType Type { get; set; } = UserType.USER;
        [Column(TypeName = "varchar(20)")]
        public string MicrosoftAccountId { get; set; } = string.Empty;

        [InverseProperty("CreatorUser")]
        public ICollection<Ticket> CreatedTickets { get; } = [];

        [InverseProperty("OperatorUser")]
        public ICollection<Ticket> AssignedTickets { get; } = [];

        public ICollection<Reply> Replies { get; } = [];

        public ICollection<Edit> Edits { get; } = [];

        public ICollection<UserPlatform> PreferredPlatforms { get; } = [];

        public ICollection<Notification> SentNotifications { get; } = [];

        public ICollection<UserNotification> ReceivedNotifications { get; } = [];

    }
}
