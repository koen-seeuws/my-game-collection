using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.AddGame;

public record AddGameCommand : ICommand
{
    public required string Title { get; init; }
    public required string Platform { get; init; }
    public Region Region { get; init; }
    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public int? ReleaseYear { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
