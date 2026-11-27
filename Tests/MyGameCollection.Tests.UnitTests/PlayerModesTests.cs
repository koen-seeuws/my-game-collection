using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.IGDB;
using MyGameCollection.Presentation.BlazorServer.Models;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class PlayerModesTests
{
    private static readonly PlayerModes CouchCoopForFour = new()
    {
        SinglePlayer = true, MaxLocalPlayers = 4, CouchCoop = true, SplitScreen = true
    };

    [Fact]
    public void AddGame_WithPlayers_StoresThemAsMultiplayer()
    {
        var collection = Empty();

        collection.AddGame("Overcooked! 2", "Nintendo Switch", players: CouchCoopForFour);

        collection.Games.Should().ContainSingle().Which.Players.Should().Be(CouchCoopForFour with { Multiplayer = true });
    }

    [Fact]
    public void AddGame_WithNothingKnownAboutPlayers_LeavesThemEmpty()
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", players: new PlayerModes());

        collection.Games[0].Players.Should().BeNull();
    }

    [Fact]
    public void UpdateGame_ChangesThePlayers()
    {
        var collection = Empty();
        var key = collection.AddGame("Overcooked! 2", "Nintendo Switch", players: CouchCoopForFour);
        var online = new PlayerModes { Multiplayer = true, MaxOnlinePlayers = 4, OnlineCoop = true };

        collection.UpdateGame(key, "Overcooked! 2", "Nintendo Switch", players: online);

        collection.Games[0].Players.Should().Be(online);
    }

    [Theory]
    [InlineData(1, null)]
    [InlineData(null, 0)]
    [InlineData(10_001, null)]
    public void AddGame_WithImpossiblePlayerCount_IsRejected(int? local, int? online)
    {
        var collection = Empty();

        var add = () => collection.AddGame("Overcooked! 2", "Nintendo Switch",
            players: new PlayerModes { MaxLocalPlayers = local, MaxOnlinePlayers = online });

        add.Should().Throw<ArgumentOutOfRangeException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddGameCommand_WithImpossiblePlayerCount_IsInvalid()
    {
        var result = new AddGameCommandInputValidator().Validate(new AddGameCommand
        {
            Title = "Overcooked! 2",
            Platform = "Nintendo Switch",
            Players = new PlayerModes { MaxLocalPlayers = 1 }
        });

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AddGameCommand.Players));
    }

    [Fact]
    public void Igdb_CombinesGameModesWithTheMostPlayersOfAnyPlatform()
    {
        var game = IgdbGame(gameModes: [1, 2, 3],
            new IgdbMultiplayerMode(Platform: 130, OfflineMax: 4, OfflineCoop: true, OfflineCoopMax: 4),
            new IgdbMultiplayerMode(Platform: 6, OfflineMax: 2, OnlineMax: 8, OnlineCoopMax: 4, OnlineCoop: true));

        IgdbPlayerModes.ForGame(game).Should().Be(new PlayerModes
        {
            SinglePlayer = true, Multiplayer = true, MaxLocalPlayers = 4, MaxOnlinePlayers = 8,
            CouchCoop = true, OnlineCoop = true
        });
    }

    [Fact]
    public void Igdb_PrefersWhatItKnowsOfThePlatform()
    {
        var game = IgdbGame(gameModes: [1, 2],
            new IgdbMultiplayerMode(Platform: 130, OfflineMax: 4, OfflineCoop: true),
            new IgdbMultiplayerMode(Platform: 6, OnlineMax: 8));

        IgdbPlayerModes.ForPlatform(game, 6).Should().Be(new IgdbPlatformPlayers(new PlayerModes
        {
            SinglePlayer = true, Multiplayer = true, MaxOnlinePlayers = 8
        }, CountsFromOtherPlatforms: false));
        // Nothing known of this platform: the whole game's details are the best guess.
        var unknown = IgdbPlayerModes.ForPlatform(game, 48);
        unknown.Players!.MaxLocalPlayers.Should().Be(4);
        unknown.CountsFromOtherPlatforms.Should().BeTrue();
    }

    [Fact]
    public void Igdb_FillsInAPlayerCountThePlatformLeavesOutFromItsOtherPlatforms()
    {
        // Far Cry 4: IGDB knows online co-op works on PS4 (48), but only how many can play on PS3 (9).
        var game = IgdbGame(gameModes: [1, 3],
            new IgdbMultiplayerMode(Platform: 9, OnlineCoopMax: 2, OnlineCoop: true),
            new IgdbMultiplayerMode(Platform: 48, OnlineCoop: true));

        IgdbPlayerModes.ForPlatform(game, 48).Should().Be(new IgdbPlatformPlayers(new PlayerModes
        {
            SinglePlayer = true, Multiplayer = true, MaxOnlinePlayers = 2, OnlineCoop = true
        }, CountsFromOtherPlatforms: true));
        IgdbPlayerModes.ForPlatform(game, 9).CountsFromOtherPlatforms.Should().BeFalse();
    }

    [Fact]
    public void Igdb_DoesNotFillInACountForAWayOfPlayingThePlatformDoesNotHave()
    {
        // Couch co-op for four on Switch (130) says nothing of how many can play on one PS4 (48).
        var game = IgdbGame(gameModes: [1, 3],
            new IgdbMultiplayerMode(Platform: 130, OfflineCoopMax: 4, OfflineCoop: true),
            new IgdbMultiplayerMode(Platform: 48, OnlineCoop: true));

        var players = IgdbPlayerModes.ForPlatform(game, 48);

        players.Players!.MaxLocalPlayers.Should().BeNull();
        players.Players.MaxOnlinePlayers.Should().BeNull();
        players.CountsFromOtherPlatforms.Should().BeFalse();
    }

    [Fact]
    public void Igdb_TreatsOnePlayerAsNoMultiplayer()
    {
        var game = IgdbGame(gameModes: [1], new IgdbMultiplayerMode(OfflineMax: 1, OnlineMax: 0));

        IgdbPlayerModes.ForGame(game).Should().Be(new PlayerModes { SinglePlayer = true });
    }

    [Fact]
    public void Igdb_WithoutAnyModes_KnowsNothing()
    {
        IgdbPlayerModes.ForGame(IgdbGame(gameModes: null)).Should().BeNull();
    }

    [Fact]
    public void GameForm_LeavesOutMultiplayerDetailsWhenMultiplayerIsUnticked()
    {
        var form = new GameFormModel { Title = "Overcooked! 2", Platform = "Nintendo Switch", Players = CouchCoopForFour };

        form.Multiplayer = false;

        form.Players.Should().Be(new PlayerModes { SinglePlayer = true });
    }

    private static IgdbGame IgdbGame(long[]? gameModes, params IgdbMultiplayerMode[] modes) =>
        new(Id: 1, Name: "Overcooked! 2", VersionTitle: null, FirstReleaseDate: null, ReleaseDates: null, Cover: null,
            Platforms: null, Genres: null, GameModes: gameModes, MultiplayerModes: modes);
}
