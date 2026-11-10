using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Common.Behaviors;
using MyGameCollection.Application.Features.Barcodes.Common;

namespace MyGameCollection.Application;

public static class DependencyInjection
{
    /// <param name="mediatRLicenseKey">
    /// Optional Lucky Penny license key for MediatR. When empty, MediatR also reads the
    /// MEDIATR_LICENSE_KEY environment variable; without any key it only logs a warning.
    /// </param>
    public static IServiceCollection AddApplication(this IServiceCollection services, string? mediatRLicenseKey = null)
    {
        services.AddMediatR(cfg =>
        {
            if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
                cfg.LicenseKey = mediatRLicenseKey;

            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<GameMetadataEnricher>();
        return services;
    }
}
