using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace MyGameCollection.Application.Validation;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers every command input validator as <see cref="IValidator{T}"/> of its command.</summary>
    public static IServiceCollection AddCommandInputValidators(this IServiceCollection services) =>
        // Validators hold no state, so one instance each will do.
        services.AddValidatorsFromAssemblyContaining(typeof(CommandInputValidatorBase<>), ServiceLifetime.Singleton);
}
