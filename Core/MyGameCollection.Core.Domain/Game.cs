using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public record Game
{
    public required Guid Key { get; init; }
    public required string Title { get; init; }

    /// <summary>The system the game runs on, e.g. "Super Nintendo".</summary>
    public required string Platform { get; init; }

    /// <summary>The edition, e.g. "Game of the Year Edition" or "Pre-Udder Edition". Empty for the standard release.</summary>
    public string? Edition { get; init; }

    /// <summary>Physical copy, bought download or subscription (e.g. PS Plus).</summary>
    public GameFormat Format { get; init; } = GameFormat.Physical;

    /// <summary>Single-player, couch or online multiplayer, and how many can play. Empty when unknown.</summary>
    public PlayerModes? Players { get; init; }

    public Region Region { get; init; } = Region.Pal;
    public ItemCondition Condition { get; init; } = ItemCondition.Good;
    public Completeness Completeness { get; init; } = Completeness.Loose;
    public CollectionStatus Status { get; init; } = CollectionStatus.Owned;
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand. Empty when unknown, and for wishlist items and digital games.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }

    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
