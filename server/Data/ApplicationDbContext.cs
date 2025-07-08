using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> User { get; set; }
    public DbSet<Request> Request { get; set; }
    public DbSet<Attachment> Attachment { get; set; }
    public DbSet<Note> Note { get; set; }

}