using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions.Services.BarcodeLookup;

namespace MyGameCollection.Infrastructure.Barcodes;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the barcode (UPC/EAN) lookup in UPCitemdb, configured from the "BarcodeLookup" section.
    /// </summary>
    public static IServiceCollection AddBarcodeLookupServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BarcodeLookupOptions>(configuration.GetSection(BarcodeLookupOptions.SectionName));
        services.AddHttpClient<UpcItemDbClient>(client => client.Timeout = TimeSpan.FromSeconds(10));

        // Transient, like the typed HttpClient it uses, so a long-lived Blazor circuit doesn't pin one handler.
        services.AddTransient<IBarcodeLookupService, UpcItemDbBarcodeLookupService>();

        return services;
    }
}
