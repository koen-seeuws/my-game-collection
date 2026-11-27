using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;
using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

public partial class Consoles : IDisposable
{
    [Inject] private IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] private CurrentUser CurrentUser { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<ConsoleDto> _consoles = [];

    /// <summary>The user's games, needed to count the owned games per console.</summary>
    private IReadOnlyList<GameDto> _games = [];

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
            _consoles = [];
            _games = [];
            return;
        }

        _loading = true;
        try
        {
            _consoles = (await Dispatcher.QueryAsync(new GetMyConsolesQuery())).ToList();
            _games = (await Dispatcher.QueryAsync(new GetMyGamesQuery())).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your consoles: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    /// <summary>Platform names already in use, suggested first when entering a console.</summary>
    private List<string> PlatformsInUse() =>
        _consoles.Select(c => c.Name)
            .Concat(_games.Select(g => g.Platform))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task AddConsoleAsync()
    {
        if (await ShowConsoleDialogAsync("Add console", new ConsoleFormModel { Status = CollectionStatus.Owned }) is not { } model)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Name}' added.");
    }

    private async Task EditConsoleAsync(ConsoleDto console)
    {
        if (await ShowConsoleDialogAsync("Edit console", ConsoleFormModel.From(console)) is not { } model)
            return;

        await ExecuteAsync(model.ToUpdateCommand(console.Key), $"'{model.Name}' updated.");
    }

    private async Task DeleteConsoleAsync(ConsoleDto console)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync("Delete",
            $"Are you sure you want to delete '{console.Name}'?", yesText: "Delete", cancelText: "Cancel") == true;

        if (confirmed)
            await ExecuteAsync(new DeleteConsoleCommand { Key = console.Key }, $"'{console.Name}' deleted.");
    }

    private async Task AcquireConsoleAsync(ConsoleDto console)
    {
        var parameters = new DialogParameters<AcquireDialog> { { d => d.ItemName, console.Name } };
        var dialog = await DialogService.ShowAsync<AcquireDialog>("Got it!", parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        if (await dialog.Result is not { Canceled: false, Data: AcquireFormModel model })
            return;

        await ExecuteAsync(new AcquireConsoleCommand
        {
            Key = console.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice,
            PurchaseOrigin = model.PurchaseOrigin
        }, $"'{console.Name}' is now part of your collection!");
    }

    private async Task<ConsoleFormModel?> ShowConsoleDialogAsync(string title, ConsoleFormModel model)
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            { "Platforms", PlatformsInUse() }
        };
        var dialog = await DialogService.ShowAsync<ConsoleDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: ConsoleFormModel result } ? result : null;
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
            Snackbar.Add("That console no longer exists.", Severity.Warning);
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

    private enum ConsoleSort { Manufacturer, Name, ReleaseDate, GameCount }

    private string? _search;
    private CollectionStatus? _statusFilter;
    private ConsoleSort _sort = ConsoleSort.Manufacturer;

    private IEnumerable<ConsoleDto> VisibleConsoles
    {
        get
        {
            var consoles = _consoles.Where(MatchesSearch);
            if (_statusFilter is { } status)
                consoles = consoles.Where(c => c.Status == status);

            return _sort switch
            {
                ConsoleSort.Name => consoles.OrderBy(c => c.Name),
                ConsoleSort.ReleaseDate => consoles.OrderBy(c => c.ReleaseDate is null).ThenBy(c => c.ReleaseDate).ThenBy(c => c.Name),
                ConsoleSort.GameCount => consoles.OrderByDescending(GameCount).ThenBy(c => c.Name),
                _ => consoles.OrderBy(c => c.Manufacturer).ThenBy(c => c.ReleaseDate is null).ThenBy(c => c.ReleaseDate).ThenBy(c => c.Name)
            };
        }
    }

    private static string Subtitle(ConsoleDto console) =>
        string.Join(" · ", new[] { console.Manufacturer, console.Model, console.Colour, console.ReleaseDate?.ToString() }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    /// <summary>The number of owned games for this system (matched on platform name).</summary>
    private int GameCount(ConsoleDto console) =>
        _games.Count(g => g.Status == CollectionStatus.Owned
                         && string.Equals(g.Platform, console.Name, StringComparison.OrdinalIgnoreCase));

    private bool MatchesSearch(ConsoleDto console) =>
        string.IsNullOrWhiteSpace(_search)
        || console.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || console.Manufacturer.Contains(_search, StringComparison.OrdinalIgnoreCase)
        || (console.Model?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (console.Colour?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (console.Notes?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false);

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
