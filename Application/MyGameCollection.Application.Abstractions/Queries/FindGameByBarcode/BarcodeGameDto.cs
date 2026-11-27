using MyGameCollection.Application.Abstractions.Services.GameCatalog;

namespace MyGameCollection.Application.Abstractions.Queries.FindGameByBarcode;

public record BarcodeGameDto
{
    /// <summary>The game, as known by the catalog.</summary>
    public required CatalogGameDto Game { get; init; }

    /// <summary>The platform the boxed copy is for, in the catalog's naming. Empty when unknown.</summary>
    public string? PlatformName { get; init; }
}
