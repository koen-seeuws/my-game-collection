using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds a console to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddConsole(
        this GameCollection collection,
        string name,
        string manufacturer,
        string? model = null,
        string? colour = null,
        Region region = Region.Pal,
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
        var console = GameCollectionValidation.Validated(new GameConsole
        {
            Key = Guid.NewGuid(),
            Name = name,
            Manufacturer = manufacturer,
            Model = model,
            Colour = colour,
            Region = region,
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

        collection.Consoles = collection.Consoles.Add(console);

        return console.Key;
    }
}
