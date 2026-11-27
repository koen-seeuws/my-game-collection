using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.Igdb;
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

        IgdbPlayerModes.ForPlatform(game, 6).Should().Be(new PlayerModes
        {
            SinglePlayer = true, Multiplayer = true, MaxOnlinePlayers = 8
        });
        // Nothing known of this platform: the whole game's details are the best guess.
        IgdbPlayerModes.ForPlatform(game, 48)!.MaxLocalPlayers.Should().Be(4);
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
        new(1, "Overcooked! 2", null, null, null, null, null, gameModes, modes);
}
