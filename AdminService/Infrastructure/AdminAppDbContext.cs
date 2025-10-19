using Microsoft.EntityFrameworkCore;
using AdminService.Domain;

namespace AdminService.Infrastructure
{
    public class AdminAppDbContext : DbContext
    {
        public AdminAppDbContext(DbContextOptions<AdminAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<APIConfig> APIConfig { get; set; }
    }
}
