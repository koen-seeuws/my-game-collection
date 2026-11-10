using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Barcodes;

public static class DependencyInjection
{
    internal const string UserAgent = "MyGameCollection/1.0 (self-hosted collection tracker)";

    /// <summary>Free barcode databases. They are queried in registration order until one returns a product.</summary>
    public static IServiceCollection AddBarcodeLookup(this IServiceCollection services)
    {
        services.AddHttpClient<UpcItemDbProvider>(c =>
        {
            c.BaseAddress = new Uri("https://api.upcitemdb.com/");
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        });
        services.AddHttpClient<OpenProductsFactsProvider>(c =>
        {
            c.BaseAddress = new Uri("https://world.openproductsfacts.org/");
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        });
        services.AddTransient<IBarcodeLookupProvider>(sp => sp.GetRequiredService<UpcItemDbProvider>());
        services.AddTransient<IBarcodeLookupProvider>(sp => sp.GetRequiredService<OpenProductsFactsProvider>());
        return services;
    }
}
