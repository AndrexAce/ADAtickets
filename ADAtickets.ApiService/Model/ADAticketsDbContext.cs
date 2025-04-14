using Microsoft.EntityFrameworkCore;

namespace ADAtickets.ApiService.Model
{
    public sealed class ADAticketsDbContext(DbContextOptions<ADAticketsDbContext> options) : DbContext(options)
    {
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Edit> Edits { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<UserPlatform> UserPlatforms { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
    }
}
