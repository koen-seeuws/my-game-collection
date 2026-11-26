using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Igdb;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the IGDB game and console search, configured from the "Igdb" section
    /// (ClientId / ClientSecret, kept in user-secrets or environment variables).
    /// </summary>
    public static IServiceCollection AddIgdbServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IgdbOptions>(configuration.GetSection(IgdbOptions.SectionName));

        var timeout = TimeSpan.FromSeconds(10);
        services.AddHttpClient(IgdbTokenProvider.HttpClientName, client => client.Timeout = timeout);
        services.AddSingleton<IgdbTokenProvider>();
        services.AddHttpClient<IgdbClient>(client => client.Timeout = timeout);

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
