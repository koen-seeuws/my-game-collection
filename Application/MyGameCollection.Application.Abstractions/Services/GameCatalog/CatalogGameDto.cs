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

    /// <summary>The studios that made the game, in the catalog's naming.</summary>
    public IReadOnlyList<string> Developers { get; init; } = [];

    /// <summary>The companies that published the game, in the catalog's naming.</summary>
    public IReadOnlyList<string> Publishers { get; init; } = [];

    /// <summary>The series or franchise the game belongs to, e.g. "Super Mario". Empty when unknown.</summary>
    public string? Series { get; init; }

    /// <summary>A game of its own, a remake, a DLC... Empty when unknown.</summary>
    public GameType? Type { get; init; }

    /// <summary>The ratings the game got from the rating boards of the world (PEGI, ESRB, CERO...).</summary>
    public IReadOnlyList<CatalogAgeRatingDto> AgeRatings { get; init; } = [];

    /// <summary>How the game can be played, on any of its platforms. Empty when the catalog does not know.</summary>
    public PlayerModes? Players { get; init; }

    /// <summary>
    /// The rating printed on a box sold in <paramref name="region"/>: PEGI (or USK / ACB) for PAL, ESRB for NTSC-U,
    /// CERO for NTSC-J. For region-free and other copies, the first known of PEGI, ESRB and CERO.
    /// Empty when the catalog does not know that region's rating.
    /// </summary>
    public string? AgeRatingFor(Region region)
    {
        string[] boards = region switch
        {
            Region.Pal => ["PEGI", "USK", "ACB"],
            Region.NtscU => ["ESRB", "ClassInd"],
            Region.NtscJ => ["CERO"],
            _ => ["PEGI", "ESRB", "CERO"]
        };

        return boards
            .Select(board => AgeRatings.FirstOrDefault(r => r.Organization.Equals(board, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(rating => rating is not null)
            ?.ToString();
    }
}

public record CatalogPlatformDto
{
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }

    /// <summary>How the game can be played on this platform (versions often differ). Empty when unknown.</summary>
    public PlayerModes? Players { get; init; }

    /// <summary>When the game came out on this platform, per region.</summary>
    public IReadOnlyList<CatalogReleaseDto> Releases { get; init; } = [];

    /// <summary>
    /// The first release on this platform in <paramref name="region"/> (or worldwide). For region-free and other
    /// copies, the first release anywhere. Empty when it did not come out there, as far as the catalog knows.
    /// </summary>
    public ReleaseDate? ReleaseDateFor(Region region)
    {
        var inRegion = First(Releases.Where(r => r.Region is null || r.Region == region));
        return inRegion ?? (region is Region.RegionFree or Region.Other ? First(Releases) : null);

        static ReleaseDate? First(IEnumerable<CatalogReleaseDto> releases) =>
            releases.Select(r => (ReleaseDate?)r.ReleaseDate).Min();
    }
}
