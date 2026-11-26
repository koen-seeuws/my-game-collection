using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Contracts;

/// <summary>The editable details of a console, as sent by the add and update commands.</summary>
public record ConsoleData
{
    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public string? Model { get; init; }
    public Region Region { get; init; }
    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public int? ReleaseYear { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public string? Notes { get; init; }
}
