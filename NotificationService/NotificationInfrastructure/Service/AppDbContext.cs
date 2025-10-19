using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.NotificationInfrastructure.Service;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GeneratedNotificationRecord> GeneratedNotifications => Set<GeneratedNotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<GeneratedNotificationRecord>();
        e.HasKey(x => x.Id);
        e.Property(x => x.StudentId).HasMaxLength(50);
        e.Property(x => x.AssignmentTitle).HasMaxLength(200);
        e.Property(x => x.Title).HasMaxLength(200).IsRequired();
        e.Property(x => x.Level).HasMaxLength(20).IsRequired();
        e.Property(x => x.Message).IsRequired();
        e.Property(x => x.RawJson).IsRequired();
    }
}
