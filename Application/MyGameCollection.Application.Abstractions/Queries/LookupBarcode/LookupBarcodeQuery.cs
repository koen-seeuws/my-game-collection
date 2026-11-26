namespace MyGameCollection.Application.Abstractions.Queries.LookupBarcode;

/// <summary>
/// Looks up the product behind a UPC/EAN barcode. Replies <c>null</c> when the barcode is unknown.
/// </summary>
/// <exception cref="ArgumentException">The barcode is not a valid UPC/EAN code.</exception>
public record LookupBarcodeQuery : IQuery<BarcodeProductDto?>
{
    public required string Barcode { get; init; }
}
