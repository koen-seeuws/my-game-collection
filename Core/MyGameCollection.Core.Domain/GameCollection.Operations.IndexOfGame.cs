namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    private static int IndexOfGame(this GameCollection collection, Guid key)
    {
        var index = collection.Games.FindIndex(g => g.Key == key);

        if (index < 0)
            throw new KeyNotFoundException($"Game '{key}' not found in the collection of '{collection.AggregateKey}'.");

        return index;
    }
}
