using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Moves an accessory from the wishlist into the collection.</summary>
    public static void AcquireAccessory(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice,
        PurchaseOrigin? purchaseOrigin = null)
    {
        var index = collection.IndexOfAccessory(key);
        var accessory = collection.Accessories[index];

        if (accessory.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Accessory '{accessory.Name}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = accessory with
        {
            Status = CollectionStatus.Owned,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin
        };

        collection.Accessories = collection.Accessories.SetItem(index, GameCollectionValidation.Validated(acquired));
    }
}
