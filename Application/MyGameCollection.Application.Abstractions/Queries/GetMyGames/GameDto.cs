using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyGames;

public record GameDto
{
    public required Guid Key { get; init; }
    public required string Title { get; init; }
    public required string Platform { get; init; }

    /// <summary>The edition, e.g. "Game of the Year Edition". Empty for the standard release.</summary>
    public string? Edition { get; init; }

    /// <summary>Physical copy, bought download or subscription (e.g. PS Plus).</summary>
    public required GameFormat Format { get; init; }

    public required Region Region { get; init; }
    public required ItemCondition Condition { get; init; }
    public required Completeness Completeness { get; init; }
    public required CollectionStatus Status { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
