using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    [PrimaryKey(nameof(ReceiverUserEmail), nameof(NotificationId))]
    public class UserNotification
    {
        [Column(TypeName = "varchar(254)")]
        public string ReceiverUserEmail { get; set; } = string.Empty;
        public Guid NotificationId { get; set; } = Guid.Empty;
    }
}
