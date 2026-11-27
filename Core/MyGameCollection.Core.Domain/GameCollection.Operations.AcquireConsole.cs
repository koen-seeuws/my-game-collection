using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Moves a console from the wishlist into the collection.</summary>
    public static void AcquireConsole(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice,
        PurchaseOrigin? purchaseOrigin = null)
    {
        var index = collection.IndexOfConsole(key);
        var console = collection.Consoles[index];

        if (console.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Console '{console.Name}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = console with
        {
            Status = CollectionStatus.Owned,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin
        };

        collection.Consoles = collection.Consoles.SetItem(index, GameCollectionValidation.Validated(acquired));
    }
}
