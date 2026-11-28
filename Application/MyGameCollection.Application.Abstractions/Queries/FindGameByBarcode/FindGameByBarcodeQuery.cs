namespace MyGameCollection.Application.Abstractions.Queries.FindGameByBarcode;

/// <summary>
/// The catalog (IGDB) game printed on the box with this barcode, to prefill the game form. The barcode database
/// (ScanDex) only tells which game it is; the game itself comes from the catalog. Empty when the barcode is unknown.
/// </summary>
/// <exception cref="ArgumentException"><see cref="Barcode"/> is not an EAN or UPC barcode.</exception>
/// <exception cref="Services.CatalogUnavailableException">The barcode database or the catalog is not configured or could not be reached.</exception>
public record FindGameByBarcodeQuery : IQuery<BarcodeGameDto?>
{
    /// <summary>The EAN or UPC barcode, as typed or scanned.</summary>
    public required string Barcode { get; init; }
}
