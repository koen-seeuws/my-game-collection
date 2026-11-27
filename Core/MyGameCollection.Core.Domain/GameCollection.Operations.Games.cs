using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds a game to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddGame(
        this GameCollection collection,
        string title,
        string platform,
        Region region = Region.Pal,
        ItemCondition condition = ItemCondition.Good,
        Completeness completeness = Completeness.Loose,
        CollectionStatus status = CollectionStatus.Owned,
        int? releaseYear = null,
        DateOnly? purchaseDate = null,
        decimal? purchasePrice = null,
        string? notes = null,
        string? thumbnailUrl = null)
    {
        var game = GameCollectionValidation.Validated(new Game
        {
            Key = Guid.NewGuid(),
            Title = title,
            Platform = platform,
            Region = region,
            Condition = condition,
            Completeness = completeness,
            Status = status,
            ReleaseYear = releaseYear,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl
        });

        collection.Games = collection.Games.Add(game);

        return game.Key;
    }

    public static void UpdateGame(
        this GameCollection collection,
        Guid key,
        string title,
        string platform,
        Region region = Region.Pal,
        ItemCondition condition = ItemCondition.Good,
        Completeness completeness = Completeness.Loose,
        CollectionStatus status = CollectionStatus.Owned,
        int? releaseYear = null,
        DateOnly? purchaseDate = null,
        decimal? purchasePrice = null,
        string? notes = null,
        string? thumbnailUrl = null)
    {
        var index = collection.IndexOfGame(key);
        var updated = GameCollectionValidation.Validated(collection.Games[index] with
        {
            Title = title,
            Platform = platform,
            Region = region,
            Condition = condition,
            Completeness = completeness,
            Status = status,
            ReleaseYear = releaseYear,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl
        });

        collection.Games = collection.Games.SetItem(index, updated);
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

    private static int IndexOfGame(this GameCollection collection, Guid key)
    {
        var index = collection.Games.FindIndex(g => g.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Game '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
