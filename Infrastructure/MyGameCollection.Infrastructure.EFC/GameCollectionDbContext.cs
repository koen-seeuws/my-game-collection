using Microsoft.EntityFrameworkCore;
using MyGameCollection.Infrastructure.EFC.DataModel;

namespace MyGameCollection.Infrastructure.EFC;

public class GameCollectionDbContext : DbContext
{
    public GameCollectionDbContext(DbContextOptions<GameCollectionDbContext> options) : base(options)
    {
    }

    public DbSet<GameCollection> GameCollections => Set<GameCollection>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameConsole> Consoles => Set<GameConsole>();
    public DbSet<AddOn> AddOns => Set<AddOn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameCollectionDbContext).Assembly);
    }
}
