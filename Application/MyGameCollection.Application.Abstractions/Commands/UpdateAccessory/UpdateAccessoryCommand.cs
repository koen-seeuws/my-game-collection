using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.UpdateAccessory;

public record UpdateAccessoryCommand : ICommand
{
    public required Guid Key { get; init; }
    public required string Name { get; init; }
    public AccessoryType Type { get; init; }

    /// <summary>The system it is for; empty when it works with anything.</summary>
    public string? Platform { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }

    /// <summary>Colour or special edition, e.g. "Ocean Blue".</summary>
    public string? Colour { get; init; }

    /// <summary>How many of this exact accessory there are.</summary>
    public int Quantity { get; init; } = 1;
    public Region Region { get; init; }
    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public DateOnly? PurchaseDate { get; init; }

    /// <summary>What was paid for all of them together.</summary>
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
