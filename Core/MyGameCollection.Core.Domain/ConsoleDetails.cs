using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>Everything that describes a console in the collection, apart from its identity.</summary>
public record ConsoleDetails
{
    /// <summary>The name of the system, e.g. "Super Nintendo". Games use the same name as their platform.</summary>
    public required string Name { get; init; }

    public required string Manufacturer { get; init; }

    /// <summary>Model or revision, e.g. "SNSP-001A" or "Slim".</summary>
    public string? Model { get; init; }

    public Region Region { get; init; } = Region.Pal;
    public ItemCondition Condition { get; init; } = ItemCondition.Good;
    public Completeness Completeness { get; init; } = Completeness.Loose;
    public CollectionStatus Status { get; init; } = CollectionStatus.Owned;
    public int? ReleaseYear { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public string? Notes { get; init; }
}
