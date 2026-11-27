using Microsoft.AspNetCore.Components;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;

namespace MyGameCollection.Presentation.BlazorServer.Components.Shared;

public partial class GameCardGrid
{
    [Parameter, EditorRequired] public IEnumerable<GameDto> Games { get; set; } = [];

    [Parameter] public EventCallback<GameDto> OnEdit { get; set; }
    [Parameter] public EventCallback<GameDto> OnDelete { get; set; }
    [Parameter] public EventCallback<GameDto> OnAcquire { get; set; }

    /// <summary>"Platform · Edition · Release date", leaving out what is unknown.</summary>
    public static string Subtitle(GameDto game) =>
        string.Join(" · ", new[] { game.Platform, game.Edition, game.ReleaseDate?.ToString() }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
