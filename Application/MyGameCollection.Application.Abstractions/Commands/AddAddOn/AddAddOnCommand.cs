using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.AddAddOn;

/// <summary>Adds an add-on (DLC, expansion...) to a game in the collection.</summary>
public record AddAddOnCommand : ICommand
{
    /// <summary>The game the add-on is for.</summary>
    public required Guid GameKey { get; init; }

    public required string Title { get; init; }

    /// <summary>DLC, an expansion, a season pass...</summary>
    public AddOnType Type { get; init; }

    /// <summary>A disc or card, a download, or claimed through a subscription.</summary>
    public GameFormat Format { get; init; }

    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture of the add-on.</summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>The add-on's id in the external catalog (IGDB). Empty when not picked from the catalog.</summary>
    public long? CatalogId { get; init; }
}
