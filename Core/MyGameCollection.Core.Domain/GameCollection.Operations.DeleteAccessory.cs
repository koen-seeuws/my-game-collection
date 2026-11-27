namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void DeleteAccessory(this GameCollection collection, Guid key)
    {
        var index = collection.IndexOfAccessory(key);

        collection.Accessories = collection.Accessories.RemoveAt(index);
    }
}
