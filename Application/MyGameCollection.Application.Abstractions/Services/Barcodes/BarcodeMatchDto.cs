namespace MyGameCollection.Application.Abstractions.Services.Barcodes;

/// <summary>The catalog (IGDB) game and platform a barcode belongs to.</summary>
public record BarcodeMatchDto
{
    /// <summary>The game's id in the catalog.</summary>
    public required long CatalogGameId { get; init; }

    /// <summary>The game's name, in the catalog's naming.</summary>
    public required string GameName { get; init; }

    /// <summary>The id in the catalog of the platform the copy is for. Empty when unknown.</summary>
    public long? CatalogPlatformId { get; init; }

    /// <summary>The platform the copy is for, in the catalog's naming. Empty when unknown.</summary>
    public string? PlatformName { get; init; }
}
