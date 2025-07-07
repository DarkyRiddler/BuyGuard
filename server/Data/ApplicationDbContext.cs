using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Project> Project { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Issue> Issue { get; set; }
    public DbSet<IssueMeta> IssueMeta { get; set; }
}