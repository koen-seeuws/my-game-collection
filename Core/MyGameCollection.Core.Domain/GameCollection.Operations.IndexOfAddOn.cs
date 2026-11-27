namespace MyGameCollection.Core.Domain;

public static partial class GameCollectionOperations
{
    /// <summary>Where the add-on <paramref name="key"/> is: the index of its game, and its own index within that game.</summary>
    private static (int GameIndex, int AddOnIndex) IndexOfAddOn(this GameCollection collection, Guid key)
    {
        for (var gameIndex = 0; gameIndex < collection.Games.Count; gameIndex++)
        {
            var addOnIndex = collection.Games[gameIndex].AddOns.FindIndex(a => a.Key == key);
            if (addOnIndex >= 0)
                return (gameIndex, addOnIndex);
        }

        throw new KeyNotFoundException($"Add-on '{key}' not found in the collection of '{collection.AggregateKey}'.");
    }

    /// <summary>Replaces the add-on at the given position with <paramref name="addOn"/> (or removes it when null).</summary>
    private static void SetAddOn(this GameCollection collection, (int GameIndex, int AddOnIndex) index, AddOn? addOn)
    {
        var game = collection.Games[index.GameIndex];
        var addOns = addOn is null
            ? game.AddOns.RemoveAt(index.AddOnIndex)
            : game.AddOns.SetItem(index.AddOnIndex, addOn);

        collection.Games = collection.Games.SetItem(index.GameIndex, game with { AddOns = addOns });
    }
}
