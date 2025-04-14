using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    [PrimaryKey(nameof(UserEmail), nameof(PlatformName))]
    public class UserPlatform
    {
        [Column(TypeName = "varchar(254)")]
        public string UserEmail { get; set; } = string.Empty;
        public User User { get; set; } = new User();

        [Column(TypeName = "varchar(50)")]
        public string PlatformName { get; set; } = string.Empty;
        public Platform Platform { get; set; } = new Platform();
    }
}
