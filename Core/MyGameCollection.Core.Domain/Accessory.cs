using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>
/// A controller, memory card, light gun, amiibo... anything that goes with a console but is not a console or a game.
/// The catalog (IGDB) does not know accessories, so they are always entered by hand.
/// </summary>
public record Accessory
{
    public required Guid Key { get; init; }

    /// <summary>What it is, e.g. "DualShock 2" or "Memory Card (8 MB)".</summary>
    public required string Name { get; init; }

    public AccessoryType Type { get; init; } = AccessoryType.Controller;

    /// <summary>The system it is for, using the same name as games and consoles. Empty when it works with anything.</summary>
    public string? Platform { get; init; }

    /// <summary>Who made it, e.g. "Sony", or a third party like "Mad Catz". Empty when unknown.</summary>
    public string? Manufacturer { get; init; }

    /// <summary>Model number, e.g. "SCPH-10010".</summary>
    public string? Model { get; init; }

    /// <summary>Colour or special edition, e.g. "Ocean Blue".</summary>
    public string? Colour { get; init; }

    /// <summary>How many of this exact accessory there are (e.g. four of the same controller). At least one.</summary>
    public int Quantity { get; init; } = 1;

    public Region Region { get; init; } = Region.Pal;
    public ItemCondition Condition { get; init; } = ItemCondition.Good;
    public Completeness Completeness { get; init; } = Completeness.Loose;
    public CollectionStatus Status { get; init; } = CollectionStatus.Owned;
    public DateOnly? PurchaseDate { get; init; }

    /// <summary>What was paid for all of them together.</summary>
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand. Empty when unknown, and for wishlist items.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }

    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
