using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;
using MyGameCollection.Infrastructure.EFC.Repositories;

namespace MyGameCollection.Infrastructure.EFC;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlLitePersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GameCollectionDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Database")));

        services.AddRepositories();
        services.AddQueryHandlers();

        return services;
    }

    public static IServiceCollection AddSqlServerPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GameCollectionDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Database")));

        services.AddRepositories();
        services.AddQueryHandlers();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRepository<GameCollection, string>, GameCollectionRepository>();
        services.AddScoped<IUnitOfWork, EntityFrameworkCoreUnitOfWork>();

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
