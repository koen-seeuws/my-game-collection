namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>
/// The bounds of a game's, console's or accessory's details. The domain enforces them; the application validates incoming
/// commands against the same values so they fail early with a friendly message.
/// </summary>
public static class CollectionItemLimits
{
    public const int MaxNameLength = 200;
    public const int MaxShortTextLength = 100;
    public const int MaxNotesLength = 2000;
    public const int MaxUrlLength = 500;
    public const int MaxGenres = 10;

    /// <summary>The most developers, and the most publishers, a game can have.</summary>
    public const int MaxCompanies = 10;
    public const int MinReleaseYear = 1950;
    public const int MaxReleaseYear = 2100;

    /// <summary>Fewer than two players is no multiplayer.</summary>
    public const int MinMultiplayerPlayers = 2;

    /// <summary>Generous enough for MMOs and battle royales.</summary>
    public const int MaxPlayers = 10_000;

    /// <summary>The most identical accessories (e.g. four of the same controller) one entry can count.</summary>
    public const int MaxAccessoryQuantity = 99;
}
