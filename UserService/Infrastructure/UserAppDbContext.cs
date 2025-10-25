using Microsoft.EntityFrameworkCore; 
using UserService.Domain; 
namespace UserService.Infrastructure
{
    public class UserAppDbContext : DbContext
    {

        public UserAppDbContext(DbContextOptions<UserAppDbContext> options) : base(options) { }
        public DbSet<User> user { get; set; }

    }
}
