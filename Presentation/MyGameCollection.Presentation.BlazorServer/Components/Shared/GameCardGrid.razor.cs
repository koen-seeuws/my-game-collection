using Microsoft.AspNetCore.Components;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;

namespace MyGameCollection.Presentation.BlazorServer.Components.Shared;

public partial class GameCardGrid
{
    [Parameter, EditorRequired] public IEnumerable<GameDto> Games { get; set; } = [];

    [Parameter] public EventCallback<GameDto> OnOpen { get; set; }
    [Parameter] public EventCallback<GameDto> OnDelete { get; set; }
    [Parameter] public EventCallback<GameDto> OnAcquire { get; set; }

    /// <summary>"Platform · Edition · Release date", leaving out what is unknown.</summary>
    public static string Subtitle(GameDto game) =>
        string.Join(" · ", new[] { game.Platform, game.Edition, game.ReleaseDate?.ToString() }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    /// <summary>
    /// "By Nintendo EPD · Published by Nintendo", or just "By Nintendo" when it made and published the game itself.
    /// Empty when neither is known.
    /// </summary>
    public static string? Companies(GameDto game)
    {
        if (game.Developers.Count > 0 && game.Publishers.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(game.Developers))
            return $"By {string.Join(", ", game.Developers)}";

        var parts = new List<string>();
        if (game.Developers.Count > 0)
            parts.Add($"By {string.Join(", ", game.Developers)}");
        if (game.Publishers.Count > 0)
            parts.Add($"Published by {string.Join(", ", game.Publishers)}");

        return parts.Count > 0 ? string.Join(" · ", parts) : null;
    }
}
