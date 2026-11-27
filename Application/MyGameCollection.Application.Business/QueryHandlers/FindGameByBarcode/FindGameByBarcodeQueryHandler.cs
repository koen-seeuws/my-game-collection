using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.FindGameByBarcode;
using MyGameCollection.Application.Abstractions.Services.Barcodes;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;

namespace MyGameCollection.Application.Business.QueryHandlers.FindGameByBarcode;

/// <summary>Asks the barcode database which catalog game a barcode is, then fetches that game from the catalog.</summary>
public sealed class FindGameByBarcodeQueryHandler(IBarcodeCatalogService barcodes, IGameCatalogService catalog)
    : IQueryHandler<FindGameByBarcodeQuery, BarcodeGameDto?>
{
    public async Task<BarcodeGameDto?> HandleAsync(FindGameByBarcodeQuery query, CancellationToken cancellationToken)
    {
        var barcode = Barcode.Parse(query.Barcode);

        if (await barcodes.FindAsync(barcode, cancellationToken) is not { } match)
            return null;

        if (await catalog.GetGameAsync(match.CatalogGameId, cancellationToken) is not { } game)
            return null;

        return new BarcodeGameDto { Game = game, PlatformName = match.PlatformName };
    }
}
