using static MyGameCollection.Core.Domain.Abstractions.CollectionItemLimits;

namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>
/// Who can play a game, and how: alone, together on one console (couch / split screen) or online.
/// Catalogs such as IGDB often only know part of this, so every detail is optional.
/// </summary>
/// <remarks>Not validated on construction so forms can hold what the user typed; see <see cref="Problem"/>.</remarks>
public sealed record PlayerModes
{
    public bool SinglePlayer { get; init; }

    /// <summary>
    /// Can be played with others in some way. Set on its own when it is not known whether that is
    /// on one console or online; implied by any of the local or online details below.
    /// </summary>
    public bool Multiplayer { get; init; }

    /// <summary>The most people who can play on one console (couch, split screen); empty when unknown.</summary>
    public int? MaxLocalPlayers { get; init; }

    /// <summary>The most people who can play together online; empty when unknown.</summary>
    public int? MaxOnlinePlayers { get; init; }

    /// <summary>The campaign or game can be played together, side by side on one console.</summary>
    public bool CouchCoop { get; init; }

    /// <summary>The campaign or game can be played together online.</summary>
    public bool OnlineCoop { get; init; }

    /// <summary>Players on one console each get part of the screen.</summary>
    public bool SplitScreen { get; init; }

    /// <summary>Several people can play on one console.</summary>
    public bool HasLocalMultiplayer => MaxLocalPlayers > 1 || CouchCoop || SplitScreen;

    /// <summary>Several people can play together online.</summary>
    public bool HasOnlineMultiplayer => MaxOnlinePlayers > 1 || OnlineCoop;

    /// <summary>Nothing is known about how the game is played.</summary>
    public bool IsEmpty => this == new PlayerModes();

    /// <summary>Why these are not valid player modes, or <c>null</c> when they are.</summary>
    public string? Problem =>
        MaxLocalPlayers is < MinMultiplayerPlayers or > MaxPlayers
            ? $"The number of players on one console must be between {MinMultiplayerPlayers} and {MaxPlayers}."
        : MaxOnlinePlayers is < MinMultiplayerPlayers or > MaxPlayers
            ? $"The number of players online must be between {MinMultiplayerPlayers} and {MaxPlayers}."
        : null;

    /// <summary>
    /// The same modes with <see cref="Multiplayer"/> set whenever a local or online detail says so,
    /// or <c>null</c> when nothing is known.
    /// </summary>
    public PlayerModes? Normalised()
    {
        var normalised = this with { Multiplayer = Multiplayer || HasLocalMultiplayer || HasOnlineMultiplayer };
        return normalised.IsEmpty ? null : normalised;
    }
}
