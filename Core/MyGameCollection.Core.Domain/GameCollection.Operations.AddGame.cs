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
        IEnumerable<string>? developers = null,
        IEnumerable<string>? publishers = null,
        string? series = null,
        GameType? type = null,
        string? ageRating = null,
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
        PlayStatus? playStatus = null,
        DateOnly? completedOn = null,
        string? notes = null,
        string? thumbnailUrl = null,
        long? catalogId = null)
    {
        var game = GameCollectionValidation.Validated(new Game
        {
            Key = Guid.NewGuid(),
            Title = title,
            Platform = platform,
            Edition = edition,
            Genres = genres?.ToList() ?? [],
            Developers = developers?.ToList() ?? [],
            Publishers = publishers?.ToList() ?? [],
            Series = series,
            Type = type,
            AgeRating = ageRating,
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
            PlayStatus = playStatus,
            CompletedOn = completedOn,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl,
            CatalogId = catalogId
        });

        collection.Games = collection.Games.Add(game);

        return game.Key;
    }
}
