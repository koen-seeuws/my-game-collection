using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetGameCatalogDetails;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;
using MyGameCollection.Infrastructure.IGDB.QueryHandlers.GetGameCatalogDetails;

namespace MyGameCollection.Infrastructure.IGDB;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the IGDB game and console search and the live game details query, configured from the "Igdb" section
    /// (ClientId / ClientSecret, kept in user-secrets or environment variables).
    /// </summary>
    public static IServiceCollection AddIgdbServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IgdbOptions>(configuration.GetSection(IgdbOptions.SectionName));

        var timeout = TimeSpan.FromSeconds(10);
        services.AddHttpClient(IgdbTokenProvider.HttpClientName, client => client.Timeout = timeout);
        services.AddSingleton<IgdbTokenProvider>();
        services.AddHttpClient<IgdbClient>(client => client.Timeout = timeout);

        // Transient, like the typed HttpClient it uses, so a long-lived Blazor circuit doesn't pin one handler.
        services.AddTransient<IGameCatalogService, IgdbGameCatalogService>();

        // Live game details (rating, time to beat...) are cached in memory; see IgdbOptions.DetailsCacheDuration.
        services.AddMemoryCache();
        services.AddScoped<IQueryHandler<GetGameCatalogDetailsQuery, GameCatalogDetailsDto?>, GetGameCatalogDetailsQueryHandler>();

        return services;
    }
}
