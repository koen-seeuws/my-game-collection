using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>
/// Something that extends a game (DLC, an expansion, a season pass...), owned or wished for on its own. It cannot
/// exist without its game: it plays on the game's platform, and goes when the game is deleted.
/// </summary>
public record AddOn
{
    public required Guid Key { get; init; }

    /// <summary>The add-on's id in the external catalog (IGDB), to look it up again. Empty when not picked from the catalog.</summary>
    public long? CatalogId { get; init; }

    public required string Title { get; init; }

    public AddOnType Type { get; init; } = AddOnType.Dlc;

    /// <summary>A disc or card, a download code or download, or claimed through a subscription.</summary>
    public GameFormat Format { get; init; } = GameFormat.Digital;

    /// <summary>Only physical add-ons have a condition.</summary>
    public ItemCondition Condition { get; init; } = ItemCondition.Good;

    /// <summary>Only physical add-ons have a box.</summary>
    public Completeness Completeness { get; init; } = Completeness.Loose;

    public CollectionStatus Status { get; init; } = CollectionStatus.Owned;
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand. Empty when unknown, and for wishlist items and digital add-ons.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }

    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture of the add-on.</summary>
    public string? ThumbnailUrl { get; init; }
}
