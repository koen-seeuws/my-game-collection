using MyGameCollection.Application.Abstractions.Services.BarcodeLookup;

namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>Looks barcodes up in UPCitemdb and turns the listing into a game catalog search.</summary>
internal sealed class UpcItemDbBarcodeLookupService(UpcItemDbClient upcItemDb) : IBarcodeLookupService
{
    public async Task<BarcodeProductDto?> LookupAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (!Gtin.TryNormalize(barcode, out var normalized))
            throw new ArgumentException($"'{barcode}' is not a valid UPC/EAN barcode.", nameof(barcode));

        if (await upcItemDb.FindProductNameAsync(normalized, cancellationToken) is not { } productName)
            return null;

        return new BarcodeProductDto
        {
            Barcode = normalized,
            ProductName = productName,
            SearchTerm = ProductTitle.ToSearchTerm(productName),
            Region = Gtin.GuessRegion(normalized)
        };
    }
}
