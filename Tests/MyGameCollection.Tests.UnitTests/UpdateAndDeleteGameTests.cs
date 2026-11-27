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

        collection.Games[0].Should().BeSameAs(first);
        // By members: the genres list is a new (equal) instance after validation.
        collection.Games[1].Should()
            .BeEquivalentTo(middle with { Title = "Changed", Platform = "Game Boy", Completeness = Completeness.Sealed },
                options => options.ComparingRecordsByMembers());
        collection.Games[2].Should().BeSameAs(last);
    }

    [Fact]
    public void UpdateGame_InvalidDetails_ThrowsAndLeavesGameUnchanged()
    {
        var game = Game();
        var collection = WithGames(game);

        collection.Invoking(c => c.UpdateGame(game.Key, "", "Super Nintendo")).Should().Throw<ArgumentException>();
        collection.Games[0].Should().BeSameAs(game);
    }

    [Fact]
    public void UpdateGame_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = WithGames(Game());
        var unknownKey = Guid.NewGuid();

        collection.Invoking(c => c.UpdateGame(unknownKey, "Super Mario World", "Super Nintendo"))
            .Should().Throw<KeyNotFoundException>()
            .Which.Message.Should().Contain(unknownKey.ToString()).And.Contain(Owner);
    }

    [Fact]
    public void DeleteGame_RemovesOnlyMatchingGameAndPreservesOrder()
    {
        var first = Game("First");
        var middle = Game("Middle");
        var last = Game("Last");
        var collection = WithGames(first, middle, last);

        collection.DeleteGame(middle.Key);

        collection.Games.Should().Equal(first, last);
    }

    [Fact]
    public void DeleteGame_SameKeyTwice_SecondCallThrows()
    {
        var game = Game();
        var collection = WithGames(game);

        collection.DeleteGame(game.Key);

        collection.Invoking(c => c.DeleteGame(game.Key)).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void DeleteGame_LeavesConsolesUntouched()
    {
        var game = Game();
        var console = Console();
        var collection = new GameCollection { AggregateKey = Owner, Games = [game], Consoles = [console] };

        collection.DeleteGame(game.Key);

        collection.Consoles.Should().Equal(console);
    }
}
