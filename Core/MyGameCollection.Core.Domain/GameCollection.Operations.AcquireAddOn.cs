using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Moves an add-on from the wishlist into the collection.</summary>
    public static void AcquireAddOn(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice,
        PurchaseOrigin? purchaseOrigin = null)
    {
        var index = collection.IndexOfAddOn(key);
        var addOn = collection.Games[index.GameIndex].AddOns[index.AddOnIndex];

        if (addOn.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Add-on '{addOn.Title}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        collection.SetAddOn(index, GameCollectionValidation.Validated(addOn with
        {
            Status = CollectionStatus.Owned,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            PurchaseOrigin = purchaseOrigin
        }));
    }
}
