using System.ComponentModel.DataAnnotations;
using MediatR;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Application.Features.Barcodes.Common;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Barcodes;

internal sealed class SearchGameByTitleHandler(GameMetadataEnricher enricher) : IRequestHandler<SearchGameByTitleQuery, BarcodeLookupResult?>
{
    public async Task<BarcodeLookupResult?> Handle(SearchGameByTitleQuery request, CancellationToken cancellationToken)
    {
        var seed = new BarcodeLookupResult
        {
            Barcode = string.Empty,
            Title = request.Title.Trim(),
            PlatformId = request.PlatformId,
            Type = ItemType.Game
        };
        var enriched = await enricher.EnrichAsync(seed, cancellationToken);
        return ReferenceEquals(enriched, seed) ? null : enriched;
    }
}
