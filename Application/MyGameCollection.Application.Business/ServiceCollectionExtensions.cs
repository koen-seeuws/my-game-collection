using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Application.Business.Dispatching;

namespace MyGameCollection.Application.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameCollectionApplicationServices(this IServiceCollection services)
    {
        services.AddCommandHandlers();

        services.TryAddSingleton(TimeProvider.System);

        // Runs each use case in its own DI scope.
        services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();

        return services;
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        Type[] handlerTypes = [typeof(ICommandHandler<>), typeof(ICommandHandler<,>)];

        var registrations = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()))
                .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, implementation) in registrations)
        {
            // The real handler, resolvable only by its concrete type...
            services.AddScoped(implementation);

            // ...wrapped in a transactional decorator, which is what consumers get for the interface.
            var decoratorDefinition = service.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                ? typeof(TransactionalCommandHandler<>)
                : typeof(TransactionalCommandHandler<,>);
            var decorator = decoratorDefinition.MakeGenericType(service.GetGenericArguments());

            var unitOfWork = implementation.GetCustomAttribute<UnitOfWorkAttribute>()?.UnitOfWorkType ?? typeof(IUnitOfWork);
            services.AddScoped(service, sp => ActivatorUtilities.CreateInstance(sp, decorator,
                sp.GetRequiredService(implementation), sp.GetRequiredService(unitOfWork)));
        }

        return services;
    }
}
