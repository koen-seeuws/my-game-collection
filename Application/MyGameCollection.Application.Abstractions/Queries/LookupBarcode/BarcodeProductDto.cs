using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.LookupBarcode;

/// <summary>The product a barcode belongs to.</summary>
public record BarcodeProductDto
{
    public required string Barcode { get; init; }

    /// <summary>The product title as listed, e.g. "Super Mario 64 (Nintendo 64, 1996) - PAL".</summary>
    public required string ProductName { get; init; }

    /// <summary>The product title stripped of platform, edition and packaging noise, for searching the game catalog.</summary>
    public required string SearchTerm { get; init; }

    /// <summary>The region guessed from the barcode's country prefix, when it hints at one.</summary>
    public Region? Region { get; init; }
}
