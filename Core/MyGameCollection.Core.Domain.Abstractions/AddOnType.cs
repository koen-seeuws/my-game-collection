namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>What kind of add-on extends a game.</summary>
public enum AddOnType
{
    /// <summary>Downloadable content: a new character, level, outfit...</summary>
    Dlc,

    /// <summary>A large add-on with a story or campaign of its own, e.g. "Hearts of Stone".</summary>
    Expansion,

    /// <summary>A bundle of add-ons sold together, e.g. a map pack.</summary>
    Pack,

    /// <summary>Access to the add-ons that come out for a game over time.</summary>
    SeasonPass,

    /// <summary>Episodes, seasons, mods...</summary>
    Other
}
