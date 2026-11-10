using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Abstractions.ReadModels;

public enum ItemSortField
{
    Title,
    Platform,
    CreatedAt,
    PurchaseDate,
    PurchasePrice,
    EstimatedValue
}

/// <summary>Search criteria for the collection.</summary>
public sealed record ItemFilter
{
    public string? Search { get; init; }
    public ItemType? Type { get; init; }
    /// <summary>Exact (normalized) barcode match.</summary>
    public string? Barcode { get; init; }
    public IReadOnlyCollection<Guid> PlatformIds { get; init; } = [];
    public IReadOnlyCollection<Region> Regions { get; init; } = [];
    public IReadOnlyCollection<ItemStatus> Statuses { get; init; } = [];
    /// <summary>Only items in this condition or better (Unknown is excluded).</summary>
    public ItemCondition? MinimumCondition { get; init; }
    public bool CompleteOnly { get; init; }
    public bool? HasPhotos { get; init; }

    public ItemSortField SortBy { get; init; } = ItemSortField.Title;
    public bool Descending { get; init; }

    /// <summary>Zero-based page index.</summary>
    public int Page { get; init; }
    public int PageSize { get; init; } = 25;
}
