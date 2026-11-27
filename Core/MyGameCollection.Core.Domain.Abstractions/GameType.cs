namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>What kind of release a game is: a game of its own, a remake or remaster of one, or an add-on or bundle.</summary>
public enum GameType
{
    MainGame,
    Remake,
    Remaster,

    /// <summary>The original game with extra content, e.g. "Persona 5 Royal".</summary>
    ExpandedGame,

    /// <summary>The same game brought to another platform.</summary>
    Port,

    /// <summary>A large add-on, including ones that play without the original game.</summary>
    Expansion,

    /// <summary>Downloadable content or a pack of add-ons.</summary>
    Dlc,

    /// <summary>Several games sold together, e.g. a compilation.</summary>
    Bundle,

    /// <summary>Episodes, seasons, mods, updates...</summary>
    Other
}
