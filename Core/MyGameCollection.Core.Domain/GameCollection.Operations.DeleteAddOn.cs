namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    public static void DeleteAddOn(this GameCollection collection, Guid key) =>
        collection.SetAddOn(collection.IndexOfAddOn(key), null);
}
