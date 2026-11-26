using MyGameCollection.Core.Domain;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class AddGameTests
{
    [Fact]
    public void AddGame_OnEmptyCollection_AddsGameWithGivenDetailsAndReturnsItsKey()
    {
        var collection = Empty();
        var details = GameDetails("Sonic the Hedgehog", "Mega Drive") with { ReleaseYear = 1991, PurchasePrice = 15m };

        var key = collection.AddGame(details);

        var game = Assert.Single(collection.Games);
        Assert.Equal(key, game.Key);
        Assert.NotEqual(Guid.Empty, key);
        Assert.Equal(details, game.Details);
    }

    [Fact]
    public void AddGame_MultipleTimes_AssignsUniqueKeysAndAllowsDuplicates()
    {
        var collection = Empty();

        collection.AddGame(GameDetails());
        collection.AddGame(GameDetails());

        Assert.Equal(2, collection.Games.Count);
        Assert.NotEqual(collection.Games[0].Key, collection.Games[1].Key);
    }

    [Fact]
    public void AddGame_AppendsAndKeepsExistingGames()
    {
        var existing = Game("Existing");
        var collection = WithGames(existing);

        collection.AddGame(GameDetails("New"));

        Assert.Same(existing, collection.Games[0]);
        Assert.Equal("New", collection.Games[1].Details.Title);
    }

    [Fact]
    public void AddGame_TrimsTextAndDropsBlankOptionalText()
    {
        var collection = Empty();

        collection.AddGame(GameDetails("  Zelda  ", "  NES ") with { Notes = "   " });

        var details = collection.Games[0].Details;
        Assert.Equal("Zelda", details.Title);
        Assert.Equal("NES", details.Platform);
        Assert.Null(details.Notes);
    }

    [Theory]
    [InlineData("", "NES")]
    [InlineData("   ", "NES")]
    [InlineData("Zelda", "")]
    public void AddGame_WithoutTitleOrPlatform_ThrowsAndLeavesCollectionUnchanged(string title, string platform)
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddGame(GameDetails(title, platform)));
        Assert.Empty(collection.Games);
    }

    [Fact]
    public void AddGame_WithTooLongTitle_Throws()
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddGame(GameDetails(new string('x', 201))));
    }

    [Fact]
    public void AddGame_WithNegativePrice_Throws()
    {
        var collection = Empty();

        Assert.Throws<ArgumentOutOfRangeException>(() => collection.AddGame(GameDetails() with { PurchasePrice = -1m }));
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(2101)]
    public void AddGame_WithImplausibleReleaseYear_Throws(int year)
    {
        var collection = Empty();

        Assert.Throws<ArgumentOutOfRangeException>(() => collection.AddGame(GameDetails() with { ReleaseYear = year }));
    }

    [Fact]
    public void AddGame_ReplacesListInstance_LeavingPreviousSnapshotUntouched()
    {
        var collection = Empty();
        var before = collection.Games;

        collection.AddGame(GameDetails());

        Assert.Empty(before);
        Assert.NotSame(before, collection.Games);
    }
}
