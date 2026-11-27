using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class UpdateAndDeleteGameTests
{
    [Fact]
    public void UpdateGame_ExistingKey_ReplacesDetailsAndKeepsKeyAndPosition()
    {
        var first = Game("First");
        var middle = Game("Middle");
        var last = Game("Last");
        var collection = WithGames(first, middle, last);

        collection.UpdateGame(middle.Key, "Changed", "Game Boy", completeness: Completeness.Sealed);

        Assert.Same(first, collection.Games[0]);
        Assert.Equal(middle with { Title = "Changed", Platform = "Game Boy", Completeness = Completeness.Sealed },
            collection.Games[1]);
        Assert.Same(last, collection.Games[2]);
    }

    [Fact]
    public void UpdateGame_InvalidDetails_ThrowsAndLeavesGameUnchanged()
    {
        var game = Game();
        var collection = WithGames(game);

        Assert.Throws<ArgumentException>(() => collection.UpdateGame(game.Key, "", "Super Nintendo"));
        Assert.Same(game, collection.Games[0]);
    }

    [Fact]
    public void UpdateGame_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = WithGames(Game());
        var unknownKey = Guid.NewGuid();

        var ex = Assert.Throws<KeyNotFoundException>(() => collection.UpdateGame(unknownKey, "Super Mario World", "Super Nintendo"));

        Assert.Contains(unknownKey.ToString(), ex.Message);
        Assert.Contains(Owner, ex.Message);
    }

    [Fact]
    public void DeleteGame_RemovesOnlyMatchingGameAndPreservesOrder()
    {
        var first = Game("First");
        var middle = Game("Middle");
        var last = Game("Last");
        var collection = WithGames(first, middle, last);

        collection.DeleteGame(middle.Key);

        Assert.Equal([first, last], collection.Games);
    }

    [Fact]
    public void DeleteGame_SameKeyTwice_SecondCallThrows()
    {
        var game = Game();
        var collection = WithGames(game);

        collection.DeleteGame(game.Key);

        Assert.Throws<KeyNotFoundException>(() => collection.DeleteGame(game.Key));
    }

    [Fact]
    public void DeleteGame_LeavesConsolesUntouched()
    {
        var game = Game();
        var console = Console();
        var collection = new GameCollection { AggregateKey = Owner, Games = [game], Consoles = [console] };

        collection.DeleteGame(game.Key);

        Assert.Equal([console], collection.Consoles);
    }
}
