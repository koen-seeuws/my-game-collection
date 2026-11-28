using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions.Services.Barcodes;

namespace MyGameCollection.Infrastructure.ScanDex;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ScanDex barcode lookup, configured from the "ScanDex" section (AccessToken, kept in
    /// user-secrets or environment variables).
    /// </summary>
    public static IServiceCollection AddScanDexServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ScanDexOptions>(configuration.GetSection(ScanDexOptions.SectionName));

        // Transient through the typed HttpClient, so a long-lived Blazor circuit doesn't pin one handler.
        services.AddHttpClient<IBarcodeCatalogService, ScanDexBarcodeCatalogService>(client =>
            client.Timeout = TimeSpan.FromSeconds(10));

        return services;
    }
}
