using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.AddConsole;

public record AddConsoleCommand : ICommand
{
    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public string? Model { get; init; }
    public Region Region { get; init; }
    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
