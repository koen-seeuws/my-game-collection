using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyGameCollection.Infrastructure.Persistence;

/// <summary>Used by `dotnet ef` so migrations can be created without running the web app.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=mygamecollection;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new AppDbContext(options, TimeProvider.System);
    }
}
