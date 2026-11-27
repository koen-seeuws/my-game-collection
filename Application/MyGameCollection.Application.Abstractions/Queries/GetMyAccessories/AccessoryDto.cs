using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyAccessories;

public record AccessoryDto
{
    public required Guid Key { get; init; }
    public required string Name { get; init; }
    public required AccessoryType Type { get; init; }

    /// <summary>The system it is for; empty when it works with anything.</summary>
    public string? Platform { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Colour { get; init; }
    public required int Quantity { get; init; }
    public required Region Region { get; init; }
    public required ItemCondition Condition { get; init; }
    public required Completeness Completeness { get; init; }
    public required CollectionStatus Status { get; init; }
    public DateOnly? PurchaseDate { get; init; }

    /// <summary>What was paid for all of them together.</summary>
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
