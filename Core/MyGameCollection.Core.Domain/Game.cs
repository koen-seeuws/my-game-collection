using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public record Game
{
    public required Guid Key { get; init; }
    public required string Title { get; init; }

    /// <summary>The system the game runs on, e.g. "Super Nintendo".</summary>
    public required string Platform { get; init; }

    public Region Region { get; init; } = Region.Pal;
    public ItemCondition Condition { get; init; } = ItemCondition.Good;
    public Completeness Completeness { get; init; } = Completeness.Loose;
    public CollectionStatus Status { get; init; } = CollectionStatus.Owned;
    public int? ReleaseYear { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
