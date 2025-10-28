using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SubmissionService.Domain;

namespace SubmissionService.Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       // => optionsBuilder.UseSqlServer("Server=HAI2\\SQLEXPRESS;Database=Submission;Trusted_Connection=True;TrustServerCertificate=True");
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=APSASSubmissionDb;Integrated Security=True;Trust Server Certificate=True");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__12696A62150DDB19");

            entity.Property(e => e.LanguageId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Results__976902083B2A0C15");

            entity.HasOne(d => d.Submission).WithMany(p => p.Results).HasConstraintName("FK_SubmissionResults_Submissions");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE1258400B2E4");

            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Language).WithMany(p => p.Submissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submissions_Languages");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
