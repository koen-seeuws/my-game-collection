using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Infrastructure.Persistence.Repositories;

namespace MyGameCollection.Infrastructure.Persistence;

public static class DependencyInjection
{
    /// <summary>PostgreSQL via EF Core. Reads the connection string "ConnectionStrings:Default".</summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' is missing.");

        return services.AddPersistence(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
    }

    /// <summary>Registers persistence with custom EF options (used by tests, e.g. the InMemory provider).</summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> configure)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddDbContextFactory<AppDbContext>(configure);
        services.AddSingleton<IItemRepository, ItemRepository>();
        services.AddSingleton<IPlatformRepository, PlatformRepository>();
        return services;
    }
}
