using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Moves a game from the wishlist into the collection.</summary>
    public static void AcquireGame(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice)
    {
        var index = collection.IndexOfGame(key);
        var game = collection.Games[index];

        if (game.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Game '{game.Title}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = game with
        {
            Status = CollectionStatus.Owned,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice
        };

        collection.Games = collection.Games.SetItem(index, acquired);
    }
}
