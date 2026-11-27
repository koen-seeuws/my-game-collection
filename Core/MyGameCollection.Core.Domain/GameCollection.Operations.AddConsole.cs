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
        var console = GameCollectionValidation.Validated(new GameConsole
        {
            Key = Guid.NewGuid(),
            Name = name,
            Manufacturer = manufacturer,
            Model = model,
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

        collection.Consoles = collection.Consoles.Add(console);

        return console.Key;
    }
}
