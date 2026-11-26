namespace MyGameCollection.Application.Abstractions.Services.BarcodeLookup;

/// <summary>Finds the product behind a retail barcode.</summary>
public interface IBarcodeLookupService
{
    /// <summary>The product with this UPC/EAN barcode, or <c>null</c> when the barcode is unknown.</summary>
    /// <exception cref="ArgumentException">The barcode is not a valid UPC/EAN code.</exception>
    /// <exception cref="CatalogUnavailableException">The barcode database could not be reached.</exception>
    Task<BarcodeProductDto?> LookupAsync(string barcode, CancellationToken cancellationToken = default);
}
