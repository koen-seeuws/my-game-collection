using MyGameCollection.Application.Abstractions.Services.GameCatalog;

namespace MyGameCollection.Application.Abstractions.Queries.FindConsoleByBarcode;

/// <summary>
/// The catalog (IGDB) console the barcode belongs to, to prefill the console form. The barcode database (ScanDex)
/// only tells which platform it is; the console itself comes from the catalog. Empty when the barcode is unknown.
/// </summary>
/// <remarks>
/// ScanDex matches barcodes to games, each with the platform it is for: a game's barcode gives its platform.
/// </remarks>
/// <exception cref="ArgumentException"><see cref="Barcode"/> is not an EAN or UPC barcode.</exception>
/// <exception cref="Services.CatalogUnavailableException">The barcode database or the catalog is not configured or could not be reached.</exception>
public record FindConsoleByBarcodeQuery : IQuery<CatalogConsoleDto?>
{
    /// <summary>The EAN or UPC barcode, as typed or scanned.</summary>
    public required string Barcode { get; init; }
}
