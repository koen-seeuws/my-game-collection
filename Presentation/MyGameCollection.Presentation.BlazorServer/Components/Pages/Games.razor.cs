using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Commands.DeleteGame;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

public partial class Games : IDisposable
{
    [Inject] private IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] private CurrentUser CurrentUser { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<GameDto> _games = [];
    private IReadOnlyList<string> _platformsInUse = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser.Changed += OnUserChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (!CurrentUser.IsSignedIn)
        {
            _games = [];
            _platformsInUse = [];
            return;
        }

        _loading = true;
        try
        {
            _games = (await Dispatcher.QueryAsync(new GetMyGamesQuery())).ToList();
            var consoles = await Dispatcher.QueryAsync(new GetMyConsolesQuery());

            // Platform names already in use, suggested first when entering a game.
            _platformsInUse = consoles.Select(c => c.Name)
                .Concat(_games.Select(g => g.Platform))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your games: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task AddGameAsync()
    {
        if (await ShowGameDialogAsync("Add game", new GameFormModel { Status = CollectionStatus.Owned }) is not { } model)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Title}' added.");
    }

    private async Task EditGameAsync(GameDto game)
    {
        if (await ShowGameDialogAsync("Edit game", GameFormModel.From(game)) is not { } model)
            return;

        await ExecuteAsync(model.ToUpdateCommand(game.Key), $"'{model.Title}' updated.");
    }

    private async Task DeleteGameAsync(GameDto game)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync("Delete",
            $"Are you sure you want to delete '{game.Title}'?", yesText: "Delete", cancelText: "Cancel") == true;

        if (confirmed)
            await ExecuteAsync(new DeleteGameCommand { Key = game.Key }, $"'{game.Title}' deleted.");
    }

    private async Task AcquireGameAsync(GameDto game)
    {
        var parameters = new DialogParameters<AcquireDialog> { { d => d.ItemName, game.Title } };
        var dialog = await DialogService.ShowAsync<AcquireDialog>("Got it!", parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        if (await dialog.Result is not { Canceled: false, Data: AcquireFormModel model })
            return;

        await ExecuteAsync(new AcquireGameCommand
        {
            Key = game.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice
        }, $"'{game.Title}' is now part of your collection!");
    }

    private async Task<GameFormModel?> ShowGameDialogAsync(string title, GameFormModel model)
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            { "Platforms", _platformsInUse }
        };
        var dialog = await DialogService.ShowAsync<GameDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: GameFormModel result } ? result : null;
    }

    private async Task ExecuteAsync(ICommand command, string successMessage)
    {
        try
        {
            await Dispatcher.SendAsync(command);
            Snackbar.Add(successMessage, Severity.Success);
        }
        catch (KeyNotFoundException)
        {
            Snackbar.Add("That game no longer exists.", Severity.Warning);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            Snackbar.Add(ex.Message, Severity.Warning);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Something went wrong: {ex.Message}", Severity.Error);
        }

        await LoadAsync();
    }

    // ---- Filtering and sorting ----

    private enum GameSort { Title, ReleaseDate, RecentlyPurchased, Price }

    private string? _search;
    private CollectionStatus? _statusFilter;
    private GameFormat? _formatFilter;

    private static string FormatLabel(GameFormat? format) => format?.ToDisplayName() ?? "All formats";
    private GameSort _sort = GameSort.Title;
    private bool _groupByPlatform;

    private IEnumerable<GameDto> VisibleGames
    {
        get
        {
            var games = _games.Where(MatchesSearch);
            if (_statusFilter is { } status)
                games = games.Where(g => g.Status == status);
            if (_formatFilter is { } format)
                games = games.Where(g => g.Format == format);

            return _sort switch
            {
                GameSort.ReleaseDate => games.OrderBy(g => g.ReleaseDate is null).ThenBy(g => g.ReleaseDate).ThenBy(g => g.Title),
                GameSort.RecentlyPurchased => games.OrderByDescending(g => g.PurchaseDate ?? DateOnly.MinValue).ThenBy(g => g.Title),
                GameSort.Price => games.OrderByDescending(g => g.PurchasePrice ?? -1).ThenBy(g => g.Title),
                _ => games.OrderBy(g => g.Title, StringComparer.CurrentCultureIgnoreCase)
            };
        }
    }

    private bool MatchesSearch(GameDto game) =>
        string.IsNullOrWhiteSpace(_search)
        || game.Title.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || game.Platform.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || (game.Edition?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (game.Notes?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false);

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
