using MediatR;
using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Common;
using MyGameCollection.Application.Features.Barcodes.Common;
using MyGameCollection.Domain.Entities;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Barcodes;

internal sealed class LookupBarcodeHandler(
    IItemRepository items,
    IPlatformRepository platforms,
    IEnumerable<IBarcodeLookupProvider> barcodeProviders,
    GameMetadataEnricher enricher,
    ILogger<LookupBarcodeHandler> logger) : IRequestHandler<LookupBarcodeQuery, BarcodeLookupResult>
{
    public async Task<BarcodeLookupResult> Handle(LookupBarcodeQuery request, CancellationToken cancellationToken)
    {
        var code = BarcodeNormalizer.Normalize(request.Barcode)
                   ?? throw new ValidationException("Enter a barcode.");

        var existing = (await items.SearchAsync(new ItemFilter { Barcode = code, PageSize = 50 }, cancellationToken)).Items;
        if (existing.Count > 0 && await items.GetByIdAsync(existing[0].Id, cancellationToken) is { } owned)
            return FromOwnedItem(code, owned, existing);

        var product = await QueryProvidersAsync(code, cancellationToken);
        var barcodeRegion = BarcodeRegionDetector.Detect(code);

        if (product is null)
            return new BarcodeLookupResult { Barcode = code, Found = false, Region = barcodeRegion };

        var platformNames = (await platforms.GetAllAsync(cancellationToken))
            .SelectMany(p => p.AllNames().Select(n => new PlatformName(p.Id, n)))
            .ToList();
        var parsed = ProductTitleParser.Parse(product.Title, product.Category, platformNames);

        var result = new BarcodeLookupResult
        {
            Barcode = code,
            Found = true,
            Source = product.Source,
            RawTitle = product.Title,
            Type = parsed.Type,
            Title = parsed.Title,
            PlatformId = parsed.PlatformId,
            Region = parsed.Region ?? barcodeRegion,
            Publisher = parsed.Type == ItemType.Game ? product.Brand : null,
            CoverImageUrl = product.ImageUrl
        };

        return parsed.Type == ItemType.Game ? await enricher.EnrichAsync(result, cancellationToken) : result;
    }

    private static BarcodeLookupResult FromOwnedItem(string code, CollectionItem item, IReadOnlyList<ItemListDto> existing)
    {
        var game = item as Game;
        return new BarcodeLookupResult
        {
            Barcode = code,
            Found = true,
            Source = "Your collection",
            RawTitle = item.Title,
            Type = item.Type,
            Title = item.Title,
            PlatformId = item.PlatformId,
            Region = item.Region,
            Publisher = game?.Publisher,
            Developer = game?.Developer,
            Genre = game?.Genre,
            ReleaseYear = game?.ReleaseYear,
            CoverImageUrl = item.CoverImageUrl,
            ExistingItems = existing
        };
    }

    private async Task<BarcodeProduct?> QueryProvidersAsync(string code, CancellationToken ct)
    {
        foreach (var provider in barcodeProviders)
        {
            try
            {
                var product = await provider.LookupAsync(code, ct);
                if (product is not null && !string.IsNullOrWhiteSpace(product.Title))
                    return product;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Barcode provider {Provider} failed for {Barcode}", provider.Name, code);
            }
        }
        return null;
    }
}
