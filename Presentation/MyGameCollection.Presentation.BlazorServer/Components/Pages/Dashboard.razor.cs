using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetMyCollectionSummary;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

public partial class Dashboard : IDisposable
{
    [Inject] private IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] private CurrentUser CurrentUser { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    // The chart shows the biggest platforms; the rest are added up as "Other".
    private const int MaxChartSlices = 7;

    private CollectionSummaryDto? _summary;
    private string[] _chartLabels = [];
    private double[] _chartValues = [];
    private List<ChartSeries<double>> _chartSeries = [];

    // Neon slices that match the app's sci-fi theme (8 = MaxChartSlices + "Other").
    private readonly DonutChartOptions _chartOptions = new()
    {
        ChartPalette = ["#22D3EE", "#F472D0", "#A78BFA", "#2EF2A2", "#FFC94D", "#38BDF8", "#FF7A59", "#5B6B8C"]
    };

    private bool IsEmpty => _summary is { OwnedGames: 0, WishlistGames: 0, OwnedConsoles: 0, WishlistConsoles: 0 };

    protected override async Task OnInitializedAsync()
    {
        CurrentUser.Changed += OnUserChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (!CurrentUser.IsSignedIn)
        {
            _summary = null;
            return;
        }

        try
        {
            await LoadSummaryAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your collection: {ex.Message}", Severity.Error);
        }
    }

    private async Task LoadSummaryAsync()
    {
        _summary = await Dispatcher.QueryAsync(new GetMyCollectionSummaryQuery());

        var withGames = _summary.Platforms.Where(p => p.OwnedGames > 0).ToList();
        var slices = withGames.Take(MaxChartSlices).Select(p => (p.Platform, Count: p.OwnedGames)).ToList();
        var other = withGames.Skip(MaxChartSlices).Sum(p => p.OwnedGames);
        if (other > 0)
            slices.Add(("Other", other));

        _chartLabels = slices.Select(s => s.Platform).ToArray();
        _chartValues = slices.Select(s => (double)s.Count).ToArray();
        _chartSeries = _chartValues.AsChartDataSet();
    }

    private async Task AddGameAsync()
    {
        if (await ShowFormDialogAsync<GameDialog, GameFormModel>("Add game",
                new GameFormModel { Status = CollectionStatus.Owned }) is not { } model)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Title}' added.");
    }

    private async Task AddConsoleAsync()
    {
        if (await ShowFormDialogAsync<ConsoleDialog, ConsoleFormModel>("Add console",
                new ConsoleFormModel { Status = CollectionStatus.Owned }) is not { } model)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Name}' added.");
    }

    private async Task<TModel?> ShowFormDialogAsync<TDialog, TModel>(string title, TModel model)
        where TDialog : ComponentBase where TModel : class
    {
        var parameters = new DialogParameters
        {
            { "Model", model },
            // Platform names already in use, suggested first when entering a game or console.
            { "Platforms", _summary?.Platforms.Select(p => p.Platform).ToList() ?? [] }
        };
        var dialog = await DialogService.ShowAsync<TDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: TModel result } ? result : null;
    }

    private async Task ExecuteAsync(ICommand command, string successMessage)
    {
        try
        {
            await Dispatcher.SendAsync(command);
            Snackbar.Add(successMessage, Severity.Success);
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
