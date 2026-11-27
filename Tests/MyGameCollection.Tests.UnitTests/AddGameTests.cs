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

        var game = collection.Games.Should().ContainSingle().Subject;
        game.Key.Should().Be(key);
        key.Should().NotBeEmpty();
        game.Title.Should().Be("Sonic the Hedgehog");
        game.Platform.Should().Be("Mega Drive");
        game.ReleaseYear.Should().Be(1991);
        game.PurchasePrice.Should().Be(15m);
    }

    [Fact]
    public void AddGame_MultipleTimes_AssignsUniqueKeysAndAllowsDuplicates()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo");
        collection.AddGame("Super Mario World", "Super Nintendo");

        collection.Games.Should().HaveCount(2);
        collection.Games[0].Key.Should().NotBe(collection.Games[1].Key);
    }

    [Fact]
    public void AddGame_AppendsAndKeepsExistingGames()
    {
        var existing = Game("Existing");
        var collection = WithGames(existing);

        collection.AddGame("New", "Super Nintendo");

        collection.Games[0].Should().BeSameAs(existing);
        collection.Games[1].Title.Should().Be("New");
    }

    [Fact]
    public void AddGame_TrimsTextAndDropsBlankOptionalText()
    {
        var collection = Empty();

        collection.AddGame("  Zelda  ", "  NES ", notes: "   ");

        var game = collection.Games[0];
        game.Title.Should().Be("Zelda");
        game.Platform.Should().Be("NES");
        game.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData("", "NES")]
    [InlineData("   ", "NES")]
    [InlineData("Zelda", "")]
    public void AddGame_WithoutTitleOrPlatform_ThrowsAndLeavesCollectionUnchanged(string title, string platform)
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame(title, platform)).Should().Throw<ArgumentException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_WithTooLongTitle_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame(new string('x', 201), "Super Nintendo")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGame_WithNegativePrice_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Super Mario World", "Super Nintendo", purchasePrice: -1m))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(2101)]
    public void AddGame_WithImplausibleReleaseYear_Throws(int year)
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Super Mario World", "Super Nintendo", releaseYear: year))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddGame_WithThumbnailUrl_KeepsTrimmedUrl()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: "  https://images.igdb.com/igdb/image/upload/t_cover_big/co1.png ");

        collection.Games[0].ThumbnailUrl.Should().Be("https://images.igdb.com/igdb/image/upload/t_cover_big/co1.png");
    }

    [Fact]
    public void AddGame_WithBlankThumbnailUrl_DropsIt()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: "  ");

        collection.Games[0].ThumbnailUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative/cover.png")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/cover.png")]
    public void AddGame_WithInvalidThumbnailUrl_Throws(string url)
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Super Mario World", "Super Nintendo", thumbnailUrl: url))
            .Should().Throw<ArgumentException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_ReplacesListInstance_LeavingPreviousSnapshotUntouched()
    {
        var collection = Empty();
        var before = collection.Games;

        collection.AddGame("Super Mario World", "Super Nintendo");

        before.Should().BeEmpty();
        collection.Games.Should().NotBeSameAs(before);
    }
}
