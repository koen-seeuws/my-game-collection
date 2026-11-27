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

    public static void DeleteConsole(this GameCollection collection, Guid key)
    {
        var index = collection.IndexOfConsole(key);

        collection.Consoles = collection.Consoles.RemoveAt(index);
    }

    /// <summary>Moves a console from the wishlist into the collection.</summary>
    public static void AcquireConsole(this GameCollection collection, Guid key, DateOnly purchaseDate, decimal? purchasePrice)
    {
        var index = collection.IndexOfConsole(key);
        var console = collection.Consoles[index];

        if (console.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Console '{console.Name}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = console with
        {
            Status = CollectionStatus.Owned,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice
        };

        collection.Consoles = collection.Consoles.SetItem(index, acquired);
    }

    private static int IndexOfConsole(this GameCollection collection, Guid key)
    {
        var index = collection.Consoles.FindIndex(c => c.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Console '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
