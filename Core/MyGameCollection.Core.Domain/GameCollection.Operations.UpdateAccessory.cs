using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void UpdateAccessory(
        this GameCollection collection,
        Guid key,
        string name,
        AccessoryType type = AccessoryType.Controller,
        string? platform = null,
        string? manufacturer = null,
        string? model = null,
        string? colour = null,
        int quantity = 1,
        Region region = Region.Pal,
        ItemCondition condition = ItemCondition.Good,
        Completeness completeness = Completeness.Loose,
        CollectionStatus status = CollectionStatus.Owned,
        DateOnly? purchaseDate = null,
        decimal? purchasePrice = null,
        PurchaseOrigin? purchaseOrigin = null,
        string? notes = null,
        string? thumbnailUrl = null)
    {
        var index = collection.IndexOfAccessory(key);
        var updated = GameCollectionValidation.Validated(collection.Accessories[index] with
        {
            Name = name,
            Type = type,
            Platform = platform,
            Manufacturer = manufacturer,
            Model = model,
            Colour = colour,
            Quantity = quantity,
            Region = region,
            Condition = condition,
            Completeness = completeness,
            Status = status,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin,
            Notes = notes,
            ThumbnailUrl = thumbnailUrl
        });

        collection.Accessories = collection.Accessories.SetItem(index, updated);
    }
}
