using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds a game to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddGame(this GameCollection collection, GameDetails details)
    {
        var game = new Game
        {
            Key = Guid.NewGuid(),
            Details = GameCollectionValidation.Validated(details)
        };

        collection.Games = collection.Games.Add(game);

        return game.Key;
    }

    public static void UpdateGame(this GameCollection collection, Guid key, GameDetails details)
    {
        var index = collection.IndexOfGame(key);
        var validated = GameCollectionValidation.Validated(details);

        collection.Games = collection.Games.SetItem(index, collection.Games[index] with { Details = validated });
    }

    public static void DeleteGame(this GameCollection collection, Guid key)
    {
        var index = collection.IndexOfGame(key);

        collection.Games = collection.Games.RemoveAt(index);
    }

    /// <summary>Moves a game from the wishlist into the collection.</summary>
    public static void AcquireGame(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice)
    {
        var index = collection.IndexOfGame(key);
        var game = collection.Games[index];

        if (game.Details.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Game '{game.Details.Title}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = game with
        {
            Details = game.Details with
            {
                Status = CollectionStatus.Owned,
                PurchaseDate = purchaseDate,
                PurchasePrice = purchasePrice
            }
        };

        collection.Games = collection.Games.SetItem(index, acquired);
    }

    private static int IndexOfGame(this GameCollection collection, Guid key)
    {
        var index = collection.Games.FindIndex(g => g.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Game '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
