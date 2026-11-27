namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>
/// What kind of release a game is: a game of its own, a remake or remaster of one, or a bundle. Add-ons that need
/// the game to play (DLC, expansions) are no games: see <see cref="AddOnType"/>.
/// </summary>
public enum GameType
{
    MainGame,
    Remake,
    Remaster,

    /// <summary>The original game with extra content, e.g. "Persona 5 Royal".</summary>
    ExpandedGame,

    /// <summary>The same game brought to another platform.</summary>
    Port,

    /// <summary>A standalone expansion: a large add-on that plays without the original game.</summary>
    Expansion,

    /// <summary>Several games sold together, e.g. a compilation.</summary>
    Bundle,

    /// <summary>Mods, forks...</summary>
    Other
}
