namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>IGDB's game genres, offered when picking a game's genres by hand.</summary>
public static class KnownGenres
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Adventure",
        "Arcade",
        "Card & Board Game",
        "Fighting",
        "Hack and slash/Beat 'em up",
        "Indie",
        "MOBA",
        "Music",
        "Pinball",
        "Platform",
        "Point-and-click",
        "Puzzle",
        "Quiz/Trivia",
        "Racing",
        "Real Time Strategy (RTS)",
        "Role-playing (RPG)",
        "Shooter",
        "Simulator",
        "Sport",
        "Strategy",
        "Tactical",
        "Turn-based strategy (TBS)",
        "Visual Novel"
    ];

    /// <summary>The known genres plus any others the game already has (e.g. ones IGDB added later), sorted.</summary>
    public static IReadOnlyList<string> Including(IEnumerable<string> genres) =>
        All.Concat(genres)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
}
