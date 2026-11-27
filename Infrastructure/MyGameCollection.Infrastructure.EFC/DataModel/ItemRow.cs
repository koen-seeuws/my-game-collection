using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

/// <summary>The columns of everything that is owned or wished for: games, consoles and add-ons.</summary>
public abstract class ItemRow
{
    public int Id { get; set; }

    /// <summary>The domain key of the item (unique).</summary>
    public required Guid Key { get; set; }

    /// <summary>The item's id in the external catalog (IGDB); empty when not picked from it.</summary>
    public long? CatalogId { get; set; }

    public ItemCondition Condition { get; set; }
    public Completeness Completeness { get; set; }
    public CollectionStatus Status { get; set; }
    // The release date, as precisely as known: a year, a month or a day (see ReleaseDate in the domain).
    public int? ReleaseYear { get; set; }
    public int? ReleaseMonth { get; set; }
    public int? ReleaseDay { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public PurchaseOrigin? PurchaseOrigin { get; set; }
    public string? Notes { get; set; }
    public string? ThumbnailUrl { get; set; }

    /// <summary>Preserves the order of the items within their collection (or game, for add-ons).</summary>
    public int Position { get; set; }
}
