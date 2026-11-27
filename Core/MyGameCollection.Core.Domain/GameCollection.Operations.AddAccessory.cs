using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds an accessory to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddAccessory(
        this GameCollection collection,
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
        var accessory = GameCollectionValidation.Validated(new Accessory
        {
            Key = Guid.NewGuid(),
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

        collection.Accessories = collection.Accessories.Add(accessory);

        return accessory.Key;
    }
}
