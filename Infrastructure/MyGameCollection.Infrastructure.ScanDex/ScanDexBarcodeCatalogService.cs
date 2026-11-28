using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Services;
using MyGameCollection.Application.Abstractions.Services.Barcodes;

namespace MyGameCollection.Infrastructure.ScanDex;

/// <summary>Looks up which IGDB game a barcode belongs to on ScanDex.</summary>
internal sealed class ScanDexBarcodeCatalogService(HttpClient httpClient, IOptions<ScanDexOptions> options)
    : IBarcodeCatalogService
{
    public async Task<BarcodeMatchDto?> FindAsync(Barcode barcode, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
            throw new CatalogUnavailableException(
                "Barcode lookup is not set up yet. Configure ScanDex:AccessToken to look up barcodes.");

        using var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(settings.ApiBaseUrl, $"lookup?value={Uri.EscapeDataString(barcode.Value)}"));
        request.Headers.TryAddWithoutValidation("Authorization", settings.AccessToken);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);

            // ScanDex answers 404 for a barcode it has never seen.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new CatalogUnavailableException("ScanDex refused the access token. Check ScanDex:AccessToken.");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new CatalogUnavailableException("ScanDex is receiving too many requests right now. Try again in a moment.");

            if (!response.IsSuccessStatusCode)
                throw new CatalogUnavailableException($"ScanDex could not look up the barcode (HTTP {(int)response.StatusCode}).");

            var lookup = await response.Content.ReadFromJsonAsync<ScanDexLookup>(cancellationToken);
            return lookup is null ? null : ToMatch(lookup);
        }
        catch (HttpRequestException ex)
        {
            throw new CatalogUnavailableException("ScanDex could not be reached. Try again later.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CatalogUnavailableException("ScanDex took too long to answer. Try again later.", ex);
        }
    }

    /// <summary>The game a barcode is matched to; empty while ScanDex knows the barcode but not its game.</summary>
    internal static BarcodeMatchDto? ToMatch(ScanDexLookup lookup)
    {
        if (lookup.IgdbMetadata is not { Id: { } gameId } metadata)
            return null;

        return new BarcodeMatchDto
        {
            CatalogGameId = gameId,
            GameName = string.IsNullOrWhiteSpace(metadata.Name) ? $"IGDB game {gameId}" : metadata.Name.Trim(),
            CatalogPlatformId = metadata.Platform?.Id ?? metadata.PlatformId,
            PlatformName = string.IsNullOrWhiteSpace(metadata.Platform?.Name) ? null : metadata.Platform.Name.Trim()
        };
    }
}
