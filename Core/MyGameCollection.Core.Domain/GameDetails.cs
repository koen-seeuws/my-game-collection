using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>Everything that describes a game in the collection, apart from its identity.</summary>
public record GameDetails
{
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
}
