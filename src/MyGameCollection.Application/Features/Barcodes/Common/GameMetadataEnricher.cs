using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Application.Features.Barcodes.Common;

/// <summary>Fills in developer / publisher / genre / year / artwork from the configured game databases.</summary>
internal sealed class GameMetadataEnricher(
    IPlatformRepository platforms,
    IEnumerable<IGameMetadataProvider> providers,
    ILogger<GameMetadataEnricher> logger)
{
    public bool IsEnabled => providers.Any(p => p.IsEnabled);

    /// <summary>Returns an enriched copy, or the same instance when nothing was found.</summary>
    public async Task<BarcodeLookupResult> EnrichAsync(BarcodeLookupResult result, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(result.Title)) return result;

        IReadOnlyCollection<string> platformNames = [];
        if (result.PlatformId is { } platformId && await platforms.GetByIdAsync(platformId, ct) is { } platform)
            platformNames = platform.AllNames().ToList();

        foreach (var provider in providers.Where(p => p.IsEnabled))
        {
            try
            {
                var meta = await provider.FindAsync(result.Title, platformNames, ct);
                if (meta is null) continue;

                return result with
                {
                    Found = true,
                    Source = string.IsNullOrEmpty(result.Source) ? meta.Source : $"{result.Source} + {meta.Source}",
                    Title = meta.Title,
                    ReleaseYear = result.ReleaseYear ?? meta.ReleaseYear,
                    Developer = result.Developer ?? meta.Developer,
                    Publisher = meta.Publisher ?? result.Publisher,
                    Genre = result.Genre ?? meta.Genre,
                    CoverImageUrl = meta.CoverImageUrl ?? result.CoverImageUrl
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Metadata provider {Provider} failed for '{Title}'", provider.Name, result.Title);
            }
        }

        return result;
    }
}
