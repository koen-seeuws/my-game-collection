using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Application.Business.Dispatching;
using MyGameCollection.Application.Validation;

namespace MyGameCollection.Application.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameCollectionApplicationServices(this IServiceCollection services)
    {
        services.AddCommandInputValidators();
        services.AddCommandHandlers();

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
            var withReply = service.GetGenericTypeDefinition() == typeof(ICommandHandler<,>);

            // Innermost first: validation wraps the transaction, so invalid input never opens one.
            List<Type> decoratorDefinitions = [];
            if (!implementation.IsDefined(typeof(NonTransactionalAttribute), inherit: false))
                decoratorDefinitions.Add(withReply ? typeof(TransactionalCommandHandler<,>) : typeof(TransactionalCommandHandler<>));
            decoratorDefinitions.Add(withReply ? typeof(ValidatingCommandHandler<,>) : typeof(ValidatingCommandHandler<>));

            var decorators = decoratorDefinitions
                .Select(definition => definition.MakeGenericType(service.GetGenericArguments()))
                .ToArray();

            // The real handler, resolvable only by its concrete type...
            services.AddScoped(implementation);

            // ...wrapped in the decorators, which is what consumers get for the interface.
            services.AddScoped(service, sp => decorators.Aggregate(
                sp.GetRequiredService(implementation),
                (inner, decorator) => ActivatorUtilities.CreateInstance(sp, decorator, inner)));
        }

        return services;
    }
}
