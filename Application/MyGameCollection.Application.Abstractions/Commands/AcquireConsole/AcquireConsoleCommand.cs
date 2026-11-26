namespace MyGameCollection.Application.Abstractions.Commands.AcquireConsole;

/// <summary>Moves a wishlist item into the collection.</summary>
public record AcquireConsoleCommand : ICommand
{
    public required Guid Key { get; init; }
    public required DateOnly PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
}
