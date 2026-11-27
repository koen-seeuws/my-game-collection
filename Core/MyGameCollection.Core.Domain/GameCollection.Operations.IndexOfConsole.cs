namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    private static int IndexOfConsole(this GameCollection collection, Guid key)
    {
        var index = collection.Consoles.FindIndex(c => c.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Console '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
