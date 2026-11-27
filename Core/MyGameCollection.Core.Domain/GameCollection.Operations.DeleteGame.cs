namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void DeleteGame(this GameCollection collection, Guid key)
    {
        var index = collection.IndexOfGame(key);

        collection.Games = collection.Games.RemoveAt(index);
    }
}
