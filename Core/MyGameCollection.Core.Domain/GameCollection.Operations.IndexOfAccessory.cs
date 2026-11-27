namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    private static int IndexOfAccessory(this GameCollection collection, Guid key)
    {
        var index = collection.Accessories.FindIndex(a => a.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Accessory '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
