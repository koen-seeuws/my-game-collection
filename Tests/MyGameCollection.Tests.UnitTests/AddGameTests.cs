using MyGameCollection.Core.Domain;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class AddGameTests
{
    [Fact]
    public void AddGame_OnEmptyCollection_AddsGameWithGivenDetailsAndReturnsItsKey()
    {
        var collection = Empty();
        var key = collection.AddGame("Sonic the Hedgehog", "Mega Drive", releaseYear: 1991, purchasePrice: 15m);

        var game = Assert.Single(collection.Games);
        Assert.Equal(key, game.Key);
        Assert.NotEqual(Guid.Empty, key);
        Assert.Equal("Sonic the Hedgehog", game.Title);
        Assert.Equal("Mega Drive", game.Platform);
        Assert.Equal(1991, game.ReleaseYear);
        Assert.Equal(15m, game.PurchasePrice);
    }

    [Fact]
    public void AddGame_MultipleTimes_AssignsUniqueKeysAndAllowsDuplicates()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo");
        collection.AddGame("Super Mario World", "Super Nintendo");

        Assert.Equal(2, collection.Games.Count);
        Assert.NotEqual(collection.Games[0].Key, collection.Games[1].Key);
    }

    [Fact]
    public void AddGame_AppendsAndKeepsExistingGames()
    {
        var existing = Game("Existing");
        var collection = WithGames(existing);

        collection.AddGame("New", "Super Nintendo");

        Assert.Same(existing, collection.Games[0]);
        Assert.Equal("New", collection.Games[1].Title);
    }

    [Fact]
    public void AddGame_TrimsTextAndDropsBlankOptionalText()
    {
        var collection = Empty();

        collection.AddGame("  Zelda  ", "  NES ", notes: "   ");

        var game = collection.Games[0];
        Assert.Equal("Zelda", game.Title);
        Assert.Equal("NES", game.Platform);
        Assert.Null(game.Notes);
    }

    [Theory]
    [InlineData("", "NES")]
    [InlineData("   ", "NES")]
    [InlineData("Zelda", "")]
    public void AddGame_WithoutTitleOrPlatform_ThrowsAndLeavesCollectionUnchanged(string title, string platform)
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddGame(title, platform));
        Assert.Empty(collection.Games);
    }

    [Fact]
    public void AddGame_WithTooLongTitle_Throws()
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddGame(new string('x', 201), "Super Nintendo"));
    }

    [Fact]
    public void AddGame_WithNegativePrice_Throws()
    {
        var collection = Empty();

        Assert.Throws<ArgumentOutOfRangeException>(() => collection.AddGame("Super Mario World", "Super Nintendo", purchasePrice: -1m));
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(2101)]
    public void AddGame_WithImplausibleReleaseYear_Throws(int year)
    {
        var collection = Empty();

        Assert.Throws<ArgumentOutOfRangeException>(() => collection.AddGame("Super Mario World", "Super Nintendo", releaseYear: year));
    }

    [Fact]
    public void AddGame_WithThumbnailUrl_KeepsTrimmedUrl()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: "  https://images.igdb.com/igdb/image/upload/t_cover_big/co1.png ");

        Assert.Equal("https://images.igdb.com/igdb/image/upload/t_cover_big/co1.png", collection.Games[0].ThumbnailUrl);
    }

    [Fact]
    public void AddGame_WithBlankThumbnailUrl_DropsIt()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: "  ");

        Assert.Null(collection.Games[0].ThumbnailUrl);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative/cover.png")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/cover.png")]
    public void AddGame_WithInvalidThumbnailUrl_Throws(string url)
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: url));
        Assert.Empty(collection.Games);
    }

    [Fact]
    public void AddGame_ReplacesListInstance_LeavingPreviousSnapshotUntouched()
    {
        var collection = Empty();
        var before = collection.Games;

        collection.AddGame("Super Mario World", "Super Nintendo");

        Assert.Empty(before);
        Assert.NotSame(before, collection.Games);
    }
}
