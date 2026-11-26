using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

/// <summary>The columns games and consoles have in common.</summary>
public abstract class CollectionItem
{
    public int Id { get; set; }

    /// <summary>The domain key of the item (unique).</summary>
    public required Guid Key { get; set; }

    public Region Region { get; set; }
    public ItemCondition Condition { get; set; }
    public Completeness Completeness { get; set; }
    public CollectionStatus Status { get; set; }
    public int? ReleaseYear { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? Notes { get; set; }

    /// <summary>Preserves the order of the items within their collection.</summary>
    public int Position { get; set; }

    public int GameCollectionId { get; set; }
    public GameCollection GameCollection { get; set; } = null!;
}
