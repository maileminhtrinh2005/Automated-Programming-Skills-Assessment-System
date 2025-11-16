using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GeneratedNotificationRecord> GeneratedNotifications => Set<GeneratedNotificationRecord>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<GeneratedNotificationRecord>(e =>
            {
                e.ToTable("GeneratedNotifications");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).HasMaxLength(200);
                e.Property(x => x.Message).HasColumnType("nvarchar(max)");
                e.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
                e.Property(x => x.IsRead).HasDefaultValue(false);

            });
        }
    }
}
