using FeedbackService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ManualFeedback> ManualFeedbacks => Set<ManualFeedback>();
    public DbSet<GeneratedFeedbackRecord> GeneratedFeedbacks => Set<GeneratedFeedbackRecord>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ManualFeedback>(e =>
        {
            e.ToTable("ManualFeedbacks");
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(64).IsRequired();
            e.Property(x => x.AssignmentTitle).HasMaxLength(256).IsRequired();
            e.Property(x => x.InstructorId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        });

        b.Entity<GeneratedFeedbackRecord>(e =>
        {
            e.ToTable("GeneratedFeedbacks");
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(64).IsRequired();
            e.Property(x => x.AssignmentTitle).HasMaxLength(256).IsRequired();
            e.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Score);
            e.Property(x => x.RawJson).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
