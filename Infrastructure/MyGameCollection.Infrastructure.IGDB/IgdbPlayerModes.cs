using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Core.Domain.Abstractions.CollectionItemLimits;

namespace MyGameCollection.Infrastructure.IGDB;

/// <summary>
/// Turns IGDB's game modes (single player, multiplayer, co-op...) and multiplayer modes (per platform: how many
/// can play on one console or online, and whether co-op works there) into <see cref="PlayerModes"/>.
/// </summary>
internal static class IgdbPlayerModes
{
    // IGDB's game_modes ids.
    private const long SinglePlayer = 1;
    private const long Multiplayer = 2;
    private const long Cooperative = 3;
    private const long SplitScreen = 4;
    private const long Mmo = 5;
    private const long BattleRoyale = 6;

    /// <summary>The fields to ask for of a game.</summary>
    public const string Fields =
        "game_modes, multiplayer_modes.platform, multiplayer_modes.offlinemax, multiplayer_modes.offlinecoopmax, " +
        "multiplayer_modes.offlinecoop, multiplayer_modes.onlinemax, multiplayer_modes.onlinecoopmax, " +
        "multiplayer_modes.onlinecoop, multiplayer_modes.splitscreen";

    /// <summary>How the game can be played on any of its platforms.</summary>
    public static PlayerModes? ForGame(IgdbGame game) => From(game.GameModes, game.MultiplayerModes ?? []);

    /// <summary>
    /// How the game can be played on <paramref name="platformId"/>. Platforms often differ (e.g. more players on
    /// one console in the Switch version), so IGDB's details for that platform win; without any, those of the
    /// whole game are the best guess. When the platform's details say it can be played together on one console or
    /// online but leave out how many can, the whole game's count is the best guess for that.
    /// </summary>
    public static IgdbPlatformPlayers ForPlatform(IgdbGame game, long platformId)
    {
        var modes = game.MultiplayerModes ?? [];
        var onPlatform = modes.Where(m => m.Platform == platformId).ToList();
        var whole = From(game.GameModes, modes);

        if (onPlatform.Count == 0)
            return new IgdbPlatformPlayers(whole,
                CountsFromOtherPlatforms: whole is { MaxLocalPlayers: not null } or { MaxOnlinePlayers: not null });

        if (From(game.GameModes, onPlatform) is not { } players)
            return new IgdbPlatformPlayers(null, CountsFromOtherPlatforms: false);

        var local = players.MaxLocalPlayers is null && (players.CouchCoop || players.SplitScreen)
            ? whole?.MaxLocalPlayers
            : null;
        var online = players.MaxOnlinePlayers is null && players.OnlineCoop
            ? whole?.MaxOnlinePlayers
            : null;

        return new IgdbPlatformPlayers(
            players with
            {
                MaxLocalPlayers = players.MaxLocalPlayers ?? local,
                MaxOnlinePlayers = players.MaxOnlinePlayers ?? online
            },
            CountsFromOtherPlatforms: local is not null || online is not null);
    }

    private static PlayerModes? From(IReadOnlyList<long>? gameModes, IReadOnlyList<IgdbMultiplayerMode> modes)
    {
        var kinds = gameModes ?? [];

        return new PlayerModes
        {
            SinglePlayer = kinds.Contains(SinglePlayer),
            Multiplayer = kinds.Any(k => k is Multiplayer or Cooperative or SplitScreen or Mmo or BattleRoyale),
            MaxLocalPlayers = MostPlayers(modes.SelectMany(m => new[] { m.OfflineMax, m.OfflineCoopMax })),
            MaxOnlinePlayers = MostPlayers(modes.SelectMany(m => new[] { m.OnlineMax, m.OnlineCoopMax })),
            CouchCoop = modes.Any(m => m.OfflineCoop == true),
            OnlineCoop = modes.Any(m => m.OnlineCoop == true),
            SplitScreen = kinds.Contains(SplitScreen) || modes.Any(m => m.SplitScreen == true)
        }.Normalised();
    }

    /// <summary>The highest player count, if it is multiplayer at all (IGDB uses 0 and 1 for "none").</summary>
    private static int? MostPlayers(IEnumerable<int?> counts) =>
        counts.Max() is { } most and >= MinMultiplayerPlayers ? Math.Min(most, MaxPlayers) : null;
}

/// <param name="Players">How the game can be played on the platform; empty when unknown.</param>
/// <param name="CountsFromOtherPlatforms">
/// Some player count is not IGDB's for this platform but the most of any of the game's platforms.
/// </param>
internal sealed record IgdbPlatformPlayers(PlayerModes? Players, bool CountsFromOtherPlatforms);
