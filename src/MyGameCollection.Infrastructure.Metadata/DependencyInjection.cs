using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Metadata;

public static class DependencyInjection
{
    private const string UserAgent = "MyGameCollection/1.0 (self-hosted collection tracker)";

    /// <summary>RAWG game metadata (section "Rawg"). Stays disabled until an API key is configured.</summary>
    public static IServiceCollection AddRawgMetadata(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RawgOptions>(configuration.GetSection(RawgOptions.SectionName));
        services.AddHttpClient<RawgMetadataProvider>(c =>
        {
            c.BaseAddress = new Uri("https://api.rawg.io/api/");
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        });
        services.AddTransient<IGameMetadataProvider>(sp => sp.GetRequiredService<RawgMetadataProvider>());
        return services;
    }
}
