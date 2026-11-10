namespace MyGameCollection.Application.Abstractions;

/// <summary>A (free) external barcode database such as UPCitemdb or Open Products Facts.</summary>
public interface IBarcodeLookupProvider
{
    string Name { get; }

    Task<BarcodeProduct?> LookupAsync(string barcode, CancellationToken cancellationToken = default);
}

/// <summary>Raw product information as returned by a barcode database.</summary>
public sealed record BarcodeProduct(
    string Source,
    string Title,
    string? Brand = null,
    string? Category = null,
    string? Description = null,
    string? ImageUrl = null);
