using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Storage;

public static class DependencyInjection
{
    /// <summary>Stores photos on the local file system (section "PhotoStorage").</summary>
    public static IServiceCollection AddLocalPhotoStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PhotoStorageOptions>(configuration.GetSection(PhotoStorageOptions.SectionName));
        services.AddSingleton<IPhotoStorage, LocalPhotoStorage>();
        return services;
    }
}
