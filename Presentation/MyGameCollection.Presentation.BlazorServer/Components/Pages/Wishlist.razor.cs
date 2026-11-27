using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;
using MyGameCollection.Application.Abstractions.Commands.DeleteGame;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

public partial class Wishlist : IDisposable
{
    [Inject] private IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] private CurrentUser CurrentUser { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<GameDto> _games = [];
    private IReadOnlyList<ConsoleDto> _consoles = [];
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
            _consoles = [];
            return;
        }

        _loading = true;
        try
        {
            _games = (await Dispatcher.QueryAsync(new GetMyGamesQuery())).ToList();
            _consoles = (await Dispatcher.QueryAsync(new GetMyConsolesQuery())).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your wishlist: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    /// <summary>Platform names already in use, suggested first when entering a game or console.</summary>
    private List<string> PlatformsInUse() =>
        _consoles.Select(c => c.Name)
            .Concat(_games.Select(g => g.Platform))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    // ---- Games ----

    private async Task WishForGameAsync()
    {
        var model = await ShowFormDialogAsync<GameDialog, GameFormModel>("Add game to wishlist",
            new GameFormModel { Status = CollectionStatus.Wishlist });
        if (model is null)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Title}' added.");
    }

    private async Task EditGameAsync(GameDto game)
    {
        if (await ShowFormDialogAsync<GameDialog, GameFormModel>("Edit game", GameFormModel.From(game)) is not { } model)
            return;

        await ExecuteAsync(model.ToUpdateCommand(game.Key), $"'{model.Title}' updated.");
    }

    private async Task DeleteGameAsync(GameDto game)
    {
        if (await ConfirmDeleteAsync(game.Title))
            await ExecuteAsync(new DeleteGameCommand { Key = game.Key }, $"'{game.Title}' deleted.");
    }

    private async Task AcquireGameAsync(GameDto game)
    {
        if (await ShowAcquireDialogAsync(game.Title) is not { } model)
            return;

        await ExecuteAsync(new AcquireGameCommand
        {
            Key = game.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice
        }, $"'{game.Title}' is now part of your collection!");
    }

    // ---- Consoles ----

    private async Task WishForConsoleAsync()
    {
        var model = await ShowFormDialogAsync<ConsoleDialog, ConsoleFormModel>("Add console to wishlist",
            new ConsoleFormModel { Status = CollectionStatus.Wishlist });
        if (model is null)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Name}' added.");
    }

    private async Task EditConsoleAsync(ConsoleDto console)
    {
        if (await ShowFormDialogAsync<ConsoleDialog, ConsoleFormModel>("Edit console", ConsoleFormModel.From(console)) is not { } model)
            return;

        await ExecuteAsync(model.ToUpdateCommand(console.Key), $"'{model.Name}' updated.");
    }

    private async Task DeleteConsoleAsync(ConsoleDto console)
    {
        if (await ConfirmDeleteAsync(console.Name))
            await ExecuteAsync(new DeleteConsoleCommand { Key = console.Key }, $"'{console.Name}' deleted.");
    }

    private async Task AcquireConsoleAsync(ConsoleDto console)
    {
        if (await ShowAcquireDialogAsync(console.Name) is not { } model)
            return;

        await ExecuteAsync(new AcquireConsoleCommand
        {
            Key = console.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice
        }, $"'{console.Name}' is now part of your collection!");
    }

    // ---- Helpers ----

    private async Task<TModel?> ShowFormDialogAsync<TDialog, TModel>(string title, TModel model)
        where TDialog : ComponentBase where TModel : class
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            { "Platforms", PlatformsInUse() }
        };
        var dialog = await DialogService.ShowAsync<TDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: TModel result } ? result : null;
    }

    private async Task<AcquireFormModel?> ShowAcquireDialogAsync(string itemName)
    {
        var parameters = new DialogParameters<AcquireDialog> { { d => d.ItemName, itemName } };
        var dialog = await DialogService.ShowAsync<AcquireDialog>("Got it!", parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: AcquireFormModel result } ? result : null;
    }

    private async Task<bool> ConfirmDeleteAsync(string name) =>
        await DialogService.ShowMessageBoxAsync("Delete", $"Are you sure you want to delete '{name}'?",
            yesText: "Delete", cancelText: "Cancel") == true;

    private async Task ExecuteAsync(ICommand command, string successMessage)
    {
        try
        {
            await Dispatcher.SendAsync(command);
            Snackbar.Add(successMessage, Severity.Success);
        }
        catch (KeyNotFoundException)
        {
            Snackbar.Add("That item no longer exists.", Severity.Warning);
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

    private IEnumerable<GameDto> WishedGames =>
        _games.Where(g => g.Status == CollectionStatus.Wishlist).OrderBy(g => g.Title);

    private IEnumerable<ConsoleDto> WishedConsoles =>
        _consoles.Where(c => c.Status == CollectionStatus.Wishlist).OrderBy(c => c.Name);

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
