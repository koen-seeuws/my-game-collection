using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.LookupBarcode;
using MyGameCollection.Infrastructure.GameCatalog.Barcodes;

namespace MyGameCollection.Infrastructure.GameCatalog.QueryHandlers;

internal sealed class LookupBarcodeQueryHandler(UpcItemDbClient upcItemDb)
    : IQueryHandler<LookupBarcodeQuery, BarcodeProductDto?>
{
    public async Task<BarcodeProductDto?> HandleAsync(LookupBarcodeQuery query, CancellationToken ct)
    {
        if (!Gtin.TryNormalize(query.Barcode, out var barcode))
            throw new ArgumentException($"'{query.Barcode}' is not a valid UPC/EAN barcode.", nameof(query));

        if (await upcItemDb.FindProductNameAsync(barcode, ct) is not { } productName)
            return null;

        return new BarcodeProductDto
        {
            Barcode = barcode,
            ProductName = productName,
            SearchTerm = ProductTitle.ToSearchTerm(productName),
            Region = Gtin.GuessRegion(barcode)
        };
    }
}
