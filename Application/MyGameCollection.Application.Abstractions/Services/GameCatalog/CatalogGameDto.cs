using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>A game as known by the external catalog, used to prefill the game form.</summary>
public record CatalogGameDto
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    /// <summary>A small cover, for showing in search results.</summary>
    public string? CoverUrl { get; init; }

    /// <summary>A larger cover, stored as the game's thumbnail.</summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>The platforms the game was released on, in the catalog's naming.</summary>
    public IReadOnlyList<CatalogPlatformDto> Platforms { get; init; } = [];

    /// <summary>The game's genres, in the catalog's naming, e.g. "Role-playing (RPG)".</summary>
    public IReadOnlyList<string> Genres { get; init; } = [];

    /// <summary>How the game can be played, on any of its platforms. Empty when the catalog does not know.</summary>
    public PlayerModes? Players { get; init; }
}

public record CatalogPlatformDto
{
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }

    /// <summary>How the game can be played on this platform (versions often differ). Empty when unknown.</summary>
    public PlayerModes? Players { get; init; }
}
