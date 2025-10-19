using Microsoft.EntityFrameworkCore;
using SubmissionService.Domain;
using System.Collections.Generic;
using Result = SubmissionService.Domain.Result;

namespace SubmissionService.Infrastructure
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Submission> submissions { get; set; }
        public DbSet<Result>  results { get; set; }
    }
}


