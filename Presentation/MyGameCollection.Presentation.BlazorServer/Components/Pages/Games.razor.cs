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

    private async Task OpenGameAsync(GameDto game)
    {
        if (await ShowGameDialogAsync(game.Title, GameFormModel.From(game), game) is not { } model)
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
        var parameters = new DialogParameters<AcquireDialog> 
        {
            { d => d.ItemName, game.Title },
            { d => d.AskOrigin, game.Format == GameFormat.Physical }
        };
        var dialog = await DialogService.ShowAsync<AcquireDialog>("Got it!", parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        if (await dialog.Result is not { Canceled: false, Data: AcquireFormModel model })
            return;

        await ExecuteAsync(new AcquireGameCommand
        {
            Key = game.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice,
            PurchaseOrigin = model.PurchaseOrigin
        }, $"'{game.Title}' is now part of your collection!");
    }

    /// <summary>Shows the game form; an <paramref name="existing"/> game opens read-only, with its add-ons.</summary>
    private async Task<GameFormModel?> ShowGameDialogAsync(string title, GameFormModel model, GameDto? existing = null)
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            { "ReadOnly", existing is not null },
            { "GameKey", existing?.Key },
            { "AddOns", existing?.AddOns ?? [] },
            { "OnAddOnsChanged", EventCallback.Factory.Create(this, ReloadAsync) },
            { "Platforms", _platformsInUse },
            { "Companies", _games.SelectMany(g => g.Developers.Concat(g.Publishers)).ToList() }
        };
        var dialog = await DialogService.ShowAsync<GameDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: GameFormModel result } ? result : null;
    }

    /// <summary>Reloads the games behind an open dialog, e.g. after one of their add-ons changed.</summary>
    private async Task ReloadAsync()
    {
        await LoadAsync();
        StateHasChanged();
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

    private enum GameSort { Title, ReleaseDate, RecentlyPurchased, Price, RecentlyCompleted }

    private string? _search;
    private CollectionStatus? _statusFilter;
    private GameFormat? _formatFilter;
    private string? _genreFilter;

    private static string GenreLabel(string? genre) => genre ?? "All genres";

    /// <summary>The genres of the games in the collection, to filter on.</summary>
    private IReadOnlyList<string> GenresInUse =>
        _games.SelectMany(g => g.Genres)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    private static string FormatLabel(GameFormat? format) => format?.ToDisplayName() ?? "All formats";

    /// <summary>Ways to narrow the games down to how many can play, e.g. for a night on the couch.</summary>
    private enum PlayerFilter { Couch, CouchCoop, Online, OnlineCoop, SinglePlayer }

    private PlayerFilter? _playerFilter;

    private static string PlayerFilterLabel(PlayerFilter? filter) => filter switch
    {
        PlayerFilter.Couch => "Couch multiplayer",
        PlayerFilter.CouchCoop => "Couch co-op",
        PlayerFilter.Online => "Online multiplayer",
        PlayerFilter.OnlineCoop => "Online co-op",
        PlayerFilter.SinglePlayer => "Single-player",
        _ => "Any players"
    };

    private static bool MatchesPlayers(GameDto game, PlayerFilter filter) => game.Players is { } players && filter switch
    {
        PlayerFilter.Couch => players.HasLocalMultiplayer,
        PlayerFilter.CouchCoop => players.CouchCoop,
        PlayerFilter.Online => players.HasOnlineMultiplayer,
        PlayerFilter.OnlineCoop => players.OnlineCoop,
        _ => players.SinglePlayer
    };

    /// <summary>Ways to narrow the games down to how far they were played, e.g. the backlog of unplayed games.</summary>
    private enum PlayFilter { Backlog, Playing, OnHold, Dropped, Finished, HundredPercent, NotTracked }

    private PlayFilter? _playFilter;

    private static string PlayFilterLabel(PlayFilter? filter) => filter switch
    {
        PlayFilter.Backlog => "Backlog (not started)",
        PlayFilter.Playing => PlayStatus.Playing.ToDisplayName(),
        PlayFilter.OnHold => PlayStatus.OnHold.ToDisplayName(),
        PlayFilter.Dropped => PlayStatus.Dropped.ToDisplayName(),
        PlayFilter.Finished => "Completed or 100%",
        PlayFilter.HundredPercent => PlayStatus.HundredPercent.ToDisplayName(),
        PlayFilter.NotTracked => "Not tracked",
        _ => "Any play status"
    };

    private static bool MatchesPlay(GameDto game, PlayFilter filter) => filter switch
    {
        PlayFilter.Backlog => game.PlayStatus == PlayStatus.NotStarted,
        PlayFilter.Playing => game.PlayStatus == PlayStatus.Playing,
        PlayFilter.OnHold => game.PlayStatus == PlayStatus.OnHold,
        PlayFilter.Dropped => game.PlayStatus == PlayStatus.Dropped,
        PlayFilter.Finished => game.PlayStatus.IsFinished(),
        PlayFilter.HundredPercent => game.PlayStatus == PlayStatus.HundredPercent,
        _ => game.PlayStatus is null
    };

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
            if (_genreFilter is { } genre)
                games = games.Where(g => g.Genres.Contains(genre, StringComparer.OrdinalIgnoreCase));
            if (_playerFilter is { } playerFilter)
                games = games.Where(g => MatchesPlayers(g, playerFilter));
            if (_playFilter is { } playFilter)
                games = games.Where(g => MatchesPlay(g, playFilter));

            return _sort switch
            {
                GameSort.ReleaseDate => games.OrderBy(g => g.ReleaseDate is null).ThenBy(g => g.ReleaseDate).ThenBy(g => g.Title),
                GameSort.RecentlyPurchased => games.OrderByDescending(g => g.PurchaseDate ?? DateOnly.MinValue).ThenBy(g => g.Title),
                GameSort.Price => games.OrderByDescending(g => g.PurchasePrice ?? -1).ThenBy(g => g.Title),
                // Finished games first, the most recent completion first; finished games without a date after those.
                GameSort.RecentlyCompleted => games.OrderByDescending(g => g.PlayStatus.IsFinished())
                    .ThenByDescending(g => g.CompletedOn ?? DateOnly.MinValue).ThenBy(g => g.Title),
                _ => games.OrderBy(g => g.Title, StringComparer.CurrentCultureIgnoreCase)
            };
        }
    }

    private bool MatchesSearch(GameDto game) =>
        string.IsNullOrWhiteSpace(_search)
        || game.Title.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || game.Platform.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || (game.Edition?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (game.Series?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false)
        || game.Genres.Any(genre => genre.Contains(_search, StringComparison.OrdinalIgnoreCase))
        || game.Developers.Concat(game.Publishers).Any(company => company.Contains(_search, StringComparison.OrdinalIgnoreCase))
        || game.AddOns.Any(addOn => addOn.Title.Contains(_search, StringComparison.OrdinalIgnoreCase))
        || (game.Notes?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false);

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
