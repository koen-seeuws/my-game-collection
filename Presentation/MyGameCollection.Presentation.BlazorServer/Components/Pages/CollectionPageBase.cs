using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;
using MyGameCollection.Application.Abstractions.Commands.DeleteGame;
using MyGameCollection.Application.Abstractions.Commands.UpdateConsole;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

/// <summary>
/// Shared behaviour of the collection pages: (re)loading the signed-in user's games and consoles, and the
/// add / edit / delete / acquire flows with their dialogs and feedback.
/// </summary>
public abstract class CollectionPageBase : ComponentBase, IDisposable
{
    [Inject] protected IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] protected CurrentUser CurrentUser { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected IReadOnlyList<GameDto> Games { get; private set; } = [];
    protected IReadOnlyList<ConsoleDto> Consoles { get; private set; } = [];
    protected bool Loading { get; private set; }

    /// <summary>Platform names already in use, suggested first when entering a game or console.</summary>
    protected IEnumerable<string> PlatformsInUse =>
        Consoles.Select(c => c.Name).Concat(Games.Select(g => g.Platform)).Distinct(StringComparer.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        CurrentUser.Changed += OnUserChanged;
        await LoadAsync();
    }

    protected async Task LoadAsync()
    {
        if (!CurrentUser.IsSignedIn)
        {
            ClearData();
            return;
        }

        Loading = true;
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your collection: {ex.Message}", Severity.Error);
        }
        finally
        {
            Loading = false;
        }
    }

    /// <summary>Loads what the page shows; by default the user's games and consoles.</summary>
    protected virtual async Task LoadDataAsync()
    {
        Games = (await Dispatcher.QueryAsync(new GetMyGamesQuery())).ToList();
        Consoles = (await Dispatcher.QueryAsync(new GetMyConsolesQuery())).ToList();
    }

    /// <summary>Forgets the shown data when nobody is signed in.</summary>
    protected virtual void ClearData()
    {
        Games = [];
        Consoles = [];
    }

    // ---- Games ----

    protected async Task AddGameAsync(CollectionStatus status = CollectionStatus.Owned)
    {
        var title = status == CollectionStatus.Owned ? "Add game" : "Add game to wishlist";

        if (await ShowDialogAsync<GameDialog, GameFormModel>(title, new GameFormModel { Status = status }) is not { } model)
            return;

        await ExecuteAsync(new AddGameCommand { Data = model.ToData() }, $"'{model.Title}' added.");
    }

    protected async Task EditGameAsync(GameDto game)
    {
        if (await ShowDialogAsync<GameDialog, GameFormModel>("Edit game", GameFormModel.From(game)) is not { } model)
            return;

        await ExecuteAsync(new UpdateGameCommand { Key = game.Key, Data = model.ToData() }, $"'{model.Title}' updated.");
    }

    protected async Task DeleteGameAsync(GameDto game)
    {
        if (await ConfirmDeleteAsync(game.Title))
            await ExecuteAsync(new DeleteGameCommand { Key = game.Key }, $"'{game.Title}' deleted.");
    }

    protected async Task AcquireGameAsync(GameDto game)
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

    protected async Task AddConsoleAsync(CollectionStatus status = CollectionStatus.Owned)
    {
        var title = status == CollectionStatus.Owned ? "Add console" : "Add console to wishlist";

        if (await ShowDialogAsync<ConsoleDialog, ConsoleFormModel>(title, new ConsoleFormModel { Status = status }) is not { } model)
            return;

        await ExecuteAsync(new AddConsoleCommand { Data = model.ToData() }, $"'{model.Name}' added.");
    }

    protected async Task EditConsoleAsync(ConsoleDto console)
    {
        if (await ShowDialogAsync<ConsoleDialog, ConsoleFormModel>("Edit console", ConsoleFormModel.From(console)) is not { } model)
            return;

        await ExecuteAsync(new UpdateConsoleCommand { Key = console.Key, Data = model.ToData() }, $"'{model.Name}' updated.");
    }

    protected async Task DeleteConsoleAsync(ConsoleDto console)
    {
        if (await ConfirmDeleteAsync(console.Name))
            await ExecuteAsync(new DeleteConsoleCommand { Key = console.Key }, $"'{console.Name}' deleted.");
    }

    protected async Task AcquireConsoleAsync(ConsoleDto console)
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

    private async Task<TModel?> ShowDialogAsync<TDialog, TModel>(string title, TModel model)
        where TDialog : ComponentBase where TModel : class
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            { "Platforms", PlatformsInUse.ToList() }
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

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
