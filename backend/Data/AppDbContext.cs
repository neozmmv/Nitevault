using Microsoft.EntityFrameworkCore;
using nitevault.Models;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}