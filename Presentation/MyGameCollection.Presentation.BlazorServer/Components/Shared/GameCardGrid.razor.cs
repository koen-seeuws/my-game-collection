using Microsoft.AspNetCore.Components;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;

namespace MyGameCollection.Presentation.BlazorServer.Components.Shared;

public partial class GameCardGrid
{
    [Parameter, EditorRequired] public IEnumerable<GameDto> Games { get; set; } = [];

    [Parameter] public EventCallback<GameDto> OnEdit { get; set; }
    [Parameter] public EventCallback<GameDto> OnDelete { get; set; }
    [Parameter] public EventCallback<GameDto> OnAcquire { get; set; }

    private static string Subtitle(GameDto game) =>
        game.ReleaseYear is { } year ? $"{game.Platform} · {year}" : game.Platform;
}
