using Microsoft.EntityFrameworkCore;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence;

/// <summary>EF Core context over the data models only; the domain model never reaches EF.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<PlatformRecord> Platforms => Set<PlatformRecord>();
    public DbSet<ItemRecord> Items => Set<ItemRecord>();
    public DbSet<ItemPhotoRecord> Photos => Set<ItemPhotoRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        foreach (var entry in ChangeTracker.Entries<IAuditableRecord>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
