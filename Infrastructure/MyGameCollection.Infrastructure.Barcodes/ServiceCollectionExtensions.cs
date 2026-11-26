using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

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

        services.AddQueryHandlers();

        return services;
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var registrations = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, implementation) in registrations)
            services.AddScoped(service, implementation);

        return services;
    }
}
