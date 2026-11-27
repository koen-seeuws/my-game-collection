using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>An edition of a game as known by the external catalog, e.g. "Game of the Year Edition".</summary>
public record CatalogGameEditionDto
{
    public required long Id { get; init; }

    /// <summary>The edition's own name, e.g. "Pre-Udder Edition".</summary>
    public required string Edition { get; init; }

    public ReleaseDate? ReleaseDate { get; init; }

    /// <summary>A small cover, for showing in the edition picker.</summary>
    public string? CoverUrl { get; init; }

    /// <summary>A larger cover, stored as the game's thumbnail.</summary>
    public string? ThumbnailUrl { get; init; }
}
