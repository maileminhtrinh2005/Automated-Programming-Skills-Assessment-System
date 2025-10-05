using CourseService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
    public DbSet<Course> Courses => Set<Course>();
}
