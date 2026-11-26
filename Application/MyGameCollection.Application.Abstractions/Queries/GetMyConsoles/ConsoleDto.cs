using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;

public record ConsoleDto
{
    public required Guid Key { get; init; }
    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public string? Model { get; init; }
    public required Region Region { get; init; }
    public required ItemCondition Condition { get; init; }
    public required Completeness Completeness { get; init; }
    public required CollectionStatus Status { get; init; }
    public int? ReleaseYear { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }
}
