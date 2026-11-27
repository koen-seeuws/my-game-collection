using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.AcquireConsole;

/// <summary>Moves a wishlist item into the collection.</summary>
public record AcquireConsoleCommand : ICommand
{
    public required Guid Key { get; init; }
    public required DateOnly PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
}
