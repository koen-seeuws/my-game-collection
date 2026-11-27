namespace MyGameCollection.Application.Abstractions.Queries.GetGameCatalogDetails;

public record GameCatalogDetailsDto
{
    /// <summary>A short description of the game.</summary>
    public string? Summary { get; init; }

    /// <summary>The average of critic and player ratings, from 0 to 100. Empty when nobody rated it yet.</summary>
    public double? Rating { get; init; }

    /// <summary>How many ratings <see cref="Rating"/> is based on.</summary>
    public int RatingCount { get; init; }

    /// <summary>How long players took to finish the game. Empty when nobody reported it.</summary>
    public TimeToBeatDto? TimeToBeat { get; init; }

    /// <summary>Screenshots of the game, in the catalog's order.</summary>
    public IReadOnlyList<ScreenshotDto> Screenshots { get; init; } = [];

    /// <summary>The game's page in the catalog.</summary>
    public string? CatalogUrl { get; init; }
}

/// <summary>How long players took to finish a game, each empty when unknown.</summary>
public record TimeToBeatDto
{
    /// <summary>Rushing through the main story.</summary>
    public TimeSpan? Rushed { get; init; }

    /// <summary>The main story with some side content.</summary>
    public TimeSpan? Normal { get; init; }

    /// <summary>Everything, for 100%.</summary>
    public TimeSpan? Completionist { get; init; }

    /// <summary>How many players reported their time.</summary>
    public int Count { get; init; }
}

public record ScreenshotDto
{
    /// <summary>A small version, for a strip of screenshots.</summary>
    public required string ThumbnailUrl { get; init; }

    /// <summary>The full-size picture.</summary>
    public required string Url { get; init; }
}
