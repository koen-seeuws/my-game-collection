using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds an add-on (DLC, expansion...) to the game <paramref name="gameKey"/> and returns its key.</summary>
    public static Guid AddAddOn(
        this GameCollection collection,
        Guid gameKey,
        string title,
        AddOnType type = AddOnType.Dlc,
        GameFormat format = GameFormat.Digital,
        ItemCondition condition = ItemCondition.Good,
        Completeness completeness = Completeness.Loose,
        CollectionStatus status = CollectionStatus.Owned,
        ReleaseDate? releaseDate = null,
        DateOnly? purchaseDate = null,
        decimal? purchasePrice = null,
        PurchaseOrigin? purchaseOrigin = null,
        string? notes = null,
        string? thumbnailUrl = null,
        long? catalogId = null)
    {
        var gameIndex = collection.IndexOfGame(gameKey);
        var game = collection.Games[gameIndex];

        var addOn = GameCollectionValidation.Validated(new AddOn
        {
            Key = Guid.NewGuid(),
            Title = title,
            Type = type,
            Format = format,
            Condition = condition,
            Completeness = completeness,
            Status = status,
            ReleaseDate = releaseDate,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl,
            CatalogId = catalogId
        });

        collection.Games = collection.Games.SetItem(gameIndex, game with { AddOns = game.AddOns.Add(addOn) });

        return addOn.Key;
    }
}
