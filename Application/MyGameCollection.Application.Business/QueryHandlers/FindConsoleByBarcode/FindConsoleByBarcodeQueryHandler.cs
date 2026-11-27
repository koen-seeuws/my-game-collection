using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.FindConsoleByBarcode;
using MyGameCollection.Application.Abstractions.Services.Barcodes;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;

namespace MyGameCollection.Application.Business.QueryHandlers.FindConsoleByBarcode;

/// <summary>Asks the barcode database which catalog platform a barcode is for, then fetches that console from the catalog.</summary>
public sealed class FindConsoleByBarcodeQueryHandler(IBarcodeCatalogService barcodes, IGameCatalogService catalog)
    : IQueryHandler<FindConsoleByBarcodeQuery, CatalogConsoleDto?>
{
    public async Task<CatalogConsoleDto?> HandleAsync(FindConsoleByBarcodeQuery query, CancellationToken cancellationToken)
    {
        var barcode = Barcode.Parse(query.Barcode);

        if (await barcodes.FindAsync(barcode, cancellationToken) is not { CatalogPlatformId: { } platformId })
            return null;

        return await catalog.GetConsoleAsync(platformId, cancellationToken);
    }
}
