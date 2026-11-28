namespace MyGameCollection.Application.Abstractions.Services.Barcodes;

/// <summary>An external database (ScanDex) that knows which catalog (IGDB) game a product barcode belongs to.</summary>
public interface IBarcodeCatalogService
{
    /// <summary>
    /// The catalog game printed on the box with this EAN or UPC <paramref name="barcode"/>. Empty when the barcode is
    /// unknown, or known but not matched to a game yet.
    /// </summary>
    /// <exception cref="CatalogUnavailableException">The barcode database is not configured or could not be reached.</exception>
    Task<BarcodeMatchDto?> FindAsync(Barcode barcode, CancellationToken cancellationToken = default);
}
