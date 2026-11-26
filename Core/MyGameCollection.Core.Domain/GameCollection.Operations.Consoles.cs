using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Adds a console to the collection (or to the wishlist) and returns its key.</summary>
    public static Guid AddConsole(this GameCollection collection, ConsoleDetails details)
    {
        var console = new GameConsole
        {
            Key = Guid.NewGuid(),
            Details = GameCollectionValidation.Validated(details)
        };

        collection.Consoles = collection.Consoles.Add(console);

        return console.Key;
    }

    public static void UpdateConsole(this GameCollection collection, Guid key, ConsoleDetails details)
    {
        var index = collection.IndexOfConsole(key);
        var validated = GameCollectionValidation.Validated(details);

        collection.Consoles = collection.Consoles.SetItem(index, collection.Consoles[index] with { Details = validated });
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

        if (console.Details.Status == CollectionStatus.Owned)
            throw new InvalidOperationException($"Console '{console.Details.Name}' is already in the collection.");

        GameCollectionValidation.EnsureValidPrice(purchasePrice, nameof(purchasePrice));

        var acquired = console with
        {
            Details = console.Details with
            {
                Status = CollectionStatus.Owned,
                PurchaseDate = purchaseDate,
                PurchasePrice = purchasePrice
            }
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
