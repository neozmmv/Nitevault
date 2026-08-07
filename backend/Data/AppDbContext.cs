using Microsoft.EntityFrameworkCore;
using nitevault.Models;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FilePart> FileParts => Set<FilePart>();
    public DbSet<FileEntity> Files => Set<FileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // user defaults
        modelBuilder.Entity<User>().Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Active).HasDefaultValueSql("true");

        // refresh token defaults
        modelBuilder.Entity<RefreshToken>().Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.UserId);

        // file entity defaults
        modelBuilder.Entity<FileEntity>().Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<FileEntity>().Property(f => f.CreatedAt).HasDefaultValueSql("NOW()");
        modelBuilder.Entity<FileEntity>().HasIndex(f => f.UserId);
        modelBuilder.Entity<FileEntity>().HasIndex(f => f.FolderId);

        // file part defaults + relationship
        modelBuilder.Entity<FilePart>().Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        modelBuilder.Entity<FilePart>()
            .HasOne(p => p.File)
            .WithMany(f => f.Parts)
            .HasForeignKey(p => p.FileId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FilePart>().HasIndex(p => new { p.FileId, p.PartNumber });
    }
}