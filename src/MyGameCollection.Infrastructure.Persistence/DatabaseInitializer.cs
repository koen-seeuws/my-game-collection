using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGameCollection.Infrastructure.Persistence.Mapping;

namespace MyGameCollection.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Applies pending migrations and seeds the default platforms on first run.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        await using var db = await scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync(ct);
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        // Postgres may still be starting when the container comes up.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(ct);
                break;
            }
            catch (Exception ex) when (attempt < 10 && ex is Npgsql.NpgsqlException or System.Net.Sockets.SocketException or TimeoutException)
            {
                logger.LogWarning("Database not reachable yet (attempt {Attempt}): {Message}", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }

        if (!await db.Platforms.AnyAsync(ct))
        {
            db.Platforms.AddRange(PlatformSeed.Create().Select(PlatformMapper.ToRecord));
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default platforms");
        }
    }
}
