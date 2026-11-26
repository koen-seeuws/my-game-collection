using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Contracts;

/// <summary>The editable details of a game, as sent by the add and update commands.</summary>
public record GameData
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
}
