namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void DeleteConsole(this GameCollection collection, Guid key)
    {
        var index = collection.IndexOfConsole(key);

        collection.Consoles = collection.Consoles.RemoveAt(index);
    }
}
