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
}
