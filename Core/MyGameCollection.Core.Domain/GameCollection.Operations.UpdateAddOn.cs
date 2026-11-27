using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void UpdateAddOn(
        this GameCollection collection,
        Guid key,
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
        var index = collection.IndexOfAddOn(key);
        var addOn = collection.Games[index.GameIndex].AddOns[index.AddOnIndex];

        collection.SetAddOn(index, GameCollectionValidation.Validated(addOn with
        {
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
        }));
    }
}
