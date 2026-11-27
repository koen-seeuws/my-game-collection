using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class PlayStatusTests
{
    private static readonly DateOnly CompletedDay = new(2026, 11, 20);

    [Fact]
    public void AddGame_WithoutPlayStatus_IsNotTracked()
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo");

        var game = collection.Games.Should().ContainSingle().Subject;
        game.PlayStatus.Should().BeNull();
        game.CompletedOn.Should().BeNull();
    }

    [Theory]
    [InlineData(PlayStatus.NotStarted)]
    [InlineData(PlayStatus.Playing)]
    [InlineData(PlayStatus.OnHold)]
    [InlineData(PlayStatus.Dropped)]
    [InlineData(PlayStatus.Completed)]
    [InlineData(PlayStatus.HundredPercent)]
    public void AddGame_WithPlayStatus_StoresIt(PlayStatus playStatus)
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", playStatus: playStatus);

        collection.Games[0].PlayStatus.Should().Be(playStatus);
    }

    [Theory]
    [InlineData(PlayStatus.Completed)]
    [InlineData(PlayStatus.HundredPercent)]
    public void AddGame_Finished_KeepsTheCompletionDate(PlayStatus playStatus)
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", playStatus: playStatus, completedOn: CompletedDay);

        collection.Games[0].CompletedOn.Should().Be(CompletedDay);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PlayStatus.NotStarted)]
    [InlineData(PlayStatus.Playing)]
    [InlineData(PlayStatus.OnHold)]
    [InlineData(PlayStatus.Dropped)]
    public void AddGame_NotFinished_HasNoCompletionDate(PlayStatus? playStatus)
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", playStatus: playStatus, completedOn: CompletedDay);

        collection.Games[0].CompletedOn.Should().BeNull();
    }

    [Fact]
    public void AddGame_OnWishlist_CanStillHaveAPlayStatus()
    {
        // E.g. finished on a borrowed copy, and now wanted for the shelf.
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", status: CollectionStatus.Wishlist,
            playStatus: PlayStatus.Completed, completedOn: CompletedDay);

        collection.Games[0].PlayStatus.Should().Be(PlayStatus.Completed);
        collection.Games[0].CompletedOn.Should().Be(CompletedDay);
    }

    [Fact]
    public void AddGame_UnknownPlayStatus_Throws()
    {
        var collection = Empty();

        var act = () => collection.AddGame("Super Metroid", "Super Nintendo", playStatus: (PlayStatus)42);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("playStatus");
    }

    [Fact]
    public void UpdateGame_ReopeningAFinishedGame_DropsTheCompletionDate()
    {
        var collection = Empty();
        var key = collection.AddGame("Super Metroid", "Super Nintendo",
            playStatus: PlayStatus.Completed, completedOn: CompletedDay);

        collection.UpdateGame(key, "Super Metroid", "Super Nintendo", playStatus: PlayStatus.Playing,
            completedOn: CompletedDay);

        collection.Games[0].PlayStatus.Should().Be(PlayStatus.Playing);
        collection.Games[0].CompletedOn.Should().BeNull();
    }

    [Fact]
    public void AcquireGame_KeepsThePlayStatus()
    {
        var collection = Empty();
        var key = collection.AddGame("Super Metroid", "Super Nintendo", status: CollectionStatus.Wishlist,
            playStatus: PlayStatus.HundredPercent, completedOn: CompletedDay);

        collection.AcquireGame(key, new DateOnly(2026, 11, 27), 40m);

        collection.Games[0].PlayStatus.Should().Be(PlayStatus.HundredPercent);
        collection.Games[0].CompletedOn.Should().Be(CompletedDay);
    }

    [Fact]
    public void AddGameCommand_WithUnknownPlayStatus_IsInvalid()
    {
        var result = new AddGameCommandInputValidator().Validate(new AddGameCommand
        {
            Title = "Super Metroid",
            Platform = "Super Nintendo",
            PlayStatus = (PlayStatus)42
        });

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AddGameCommand.PlayStatus));
    }
}
