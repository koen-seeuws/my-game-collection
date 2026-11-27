using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds a game to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddGame(
        this GameCollection collection,
        string title,
        string platform,
        string? edition = null,
        IEnumerable<string>? genres = null,
        GameFormat format = GameFormat.Physical,
        PlayerModes? players = null,
        Region region = Region.Pal,
        ItemCondition condition = ItemCondition.Good,
        Completeness completeness = Completeness.Loose,
        CollectionStatus status = CollectionStatus.Owned,
        ReleaseDate? releaseDate = null,
        DateOnly? purchaseDate = null,
        decimal? purchasePrice = null,
        PurchaseOrigin? purchaseOrigin = null,
        string? notes = null,
        string? thumbnailUrl = null)
    {
        var game = GameCollectionValidation.Validated(new Game
        {
            Key = Guid.NewGuid(),
            Title = title,
            Platform = platform,
            Edition = edition,
            Genres = genres?.ToList() ?? [],
            Format = format,
            Players = players,
            Region = region,
            Condition = condition,
            Completeness = completeness,
            Status = status,
            ReleaseDate = releaseDate,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl
        });

        collection.Games = collection.Games.Add(game);

        return game.Key;
    }
}
