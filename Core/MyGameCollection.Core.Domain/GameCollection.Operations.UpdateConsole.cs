using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void UpdateConsole(
        this GameCollection collection,
        Guid key,
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
        var index = collection.IndexOfConsole(key);
        var updated = GameCollectionValidation.Validated(collection.Consoles[index] with
        {
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

        collection.Consoles = collection.Consoles.SetItem(index, updated);
    }
}
