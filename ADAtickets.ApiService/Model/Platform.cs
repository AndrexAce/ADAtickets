using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADAtickets.ApiService.Model
{
    [PrimaryKey(nameof(Name), nameof(RepositoryUrl))]
    public class Platform
    {
        [Column(TypeName = "varchar(254)")]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "varchar(4000)")]
        public string RepositoryUrl { get; set; } = string.Empty;

        public ICollection<Ticket> Tickets { get; } = [];

        public ICollection<UserPlatform> PreferredPlatforms { get; } = [];
    }
}
