using AssignmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentService.Infrastructure
{
    public class AssignmentDbContext:DbContext 
    {
        public AssignmentDbContext (DbContextOptions<AssignmentDbContext> options) : base(options) { }
        public DbSet<Assignment> assignments { get; set; }
        public DbSet<TestCase> testCases { get; set; }
        public DbSet<Resource> resources { get; set; }
    }
}
