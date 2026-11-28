using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireAccessory;
using MyGameCollection.Application.Abstractions.Commands.DeleteAccessory;
using MyGameCollection.Application.Abstractions.Queries.GetMyAccessories;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Presentation.BlazorServer.Components.Dialogs;
using MyGameCollection.Presentation.BlazorServer.Models;
using MyGameCollection.Presentation.BlazorServer.Security;

namespace MyGameCollection.Presentation.BlazorServer.Components.Pages;

public partial class Accessories : IDisposable
{
    [Inject] private IUseCaseDispatcher Dispatcher { get; set; } = default!;
    [Inject] private CurrentUser CurrentUser { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private IReadOnlyList<AccessoryDto> _accessories = [];

    /// <summary>The user's consoles, whose names are suggested first as the system an accessory works with.</summary>
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
            _accessories = [];
            _consoles = [];
            return;
        }

        _loading = true;
        try
        {
            _accessories = (await Dispatcher.QueryAsync(new GetMyAccessoriesQuery())).ToList();
            _consoles = (await Dispatcher.QueryAsync(new GetMyConsolesQuery())).ToList();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Could not load your accessories: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    /// <summary>The systems the accessories are for, alphabetically, for the filter.</summary>
    private IEnumerable<string> Platforms =>
        _accessories.Select(a => a.Platform)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase);

    /// <summary>Platform names already in use, suggested first when entering an accessory.</summary>
    private List<string> PlatformsInUse() =>
        _consoles.Select(c => c.Name)
            .Concat(Platforms)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task AddAccessoryAsync()
    {
        if (await ShowAccessoryDialogAsync("Add accessory", new AccessoryFormModel { Status = CollectionStatus.Owned }) is not { } model)
            return;

        await ExecuteAsync(model.ToAddCommand(), $"'{model.Name}' added.");
    }

    private async Task OpenAccessoryAsync(AccessoryDto accessory)
    {
        if (await ShowAccessoryDialogAsync(accessory.Name, AccessoryFormModel.From(accessory), readOnly: true) is not { } model)
            return;

        await ExecuteAsync(model.ToUpdateCommand(accessory.Key), $"'{model.Name}' updated.");
    }

    private async Task DeleteAccessoryAsync(AccessoryDto accessory)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync("Delete",
            $"Are you sure you want to delete '{accessory.Name}'?", yesText: "Delete", cancelText: "Cancel") == true;

        if (confirmed)
            await ExecuteAsync(new DeleteAccessoryCommand { Key = accessory.Key }, $"'{accessory.Name}' deleted.");
    }

    private async Task AcquireAccessoryAsync(AccessoryDto accessory)
    {
        var parameters = new DialogParameters<AcquireDialog> { { d => d.ItemName, accessory.Name } };
        var dialog = await DialogService.ShowAsync<AcquireDialog>("Got it!", parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        if (await dialog.Result is not { Canceled: false, Data: AcquireFormModel model })
            return;

        await ExecuteAsync(new AcquireAccessoryCommand
        {
            Key = accessory.Key,
            PurchaseDate = DateOnly.FromDateTime(model.PurchaseDate ?? DateTime.Today),
            PurchasePrice = model.PurchasePrice,
            PurchaseOrigin = model.PurchaseOrigin
        }, $"'{accessory.Name}' is now part of your collection!");
    }

    private async Task<AccessoryFormModel?> ShowAccessoryDialogAsync(string title, AccessoryFormModel model, bool readOnly = false)
    {
        var parameters = new DialogParameters<AccessoryDialog>
        {
            { d => d.Model, model },
            { d => d.ReadOnly, readOnly },
            { d => d.Platforms, PlatformsInUse() },
            { d => d.Manufacturers, _accessories.Select(a => a.Manufacturer).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList() }
        };
        var dialog = await DialogService.ShowAsync<AccessoryDialog>(title, parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        return await dialog.Result is { Canceled: false, Data: AccessoryFormModel result } ? result : null;
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
            Snackbar.Add("That accessory no longer exists.", Severity.Warning);
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

    private enum AccessorySort { Platform, Type, Name, RecentlyBought }

    /// <summary>The platform filter value for accessories that work with any system.</summary>
    private const string AnySystem = "\0any";

    private string? _search;
    private CollectionStatus? _statusFilter;
    private AccessoryType? _typeFilter;
    private string? _platformFilter;
    private AccessorySort _sort = AccessorySort.Platform;

    private IEnumerable<AccessoryDto> VisibleAccessories
    {
        get
        {
            var accessories = _accessories.Where(MatchesSearch);
            if (_statusFilter is { } status)
                accessories = accessories.Where(a => a.Status == status);
            if (_typeFilter is { } type)
                accessories = accessories.Where(a => a.Type == type);
            if (_platformFilter is { } platform)
                accessories = accessories.Where(a => platform == AnySystem
                    ? a.Platform is null
                    : string.Equals(a.Platform, platform, StringComparison.OrdinalIgnoreCase));

            return _sort switch
            {
                AccessorySort.Type => accessories.OrderBy(a => a.Type).ThenBy(a => a.Platform is null).ThenBy(a => a.Platform).ThenBy(a => a.Name),
                AccessorySort.Name => accessories.OrderBy(a => a.Name),
                AccessorySort.RecentlyBought => accessories.OrderBy(a => a.PurchaseDate is null).ThenByDescending(a => a.PurchaseDate).ThenBy(a => a.Name),
                _ => accessories.OrderBy(a => a.Platform is null).ThenBy(a => a.Platform).ThenBy(a => a.Type).ThenBy(a => a.Name)
            };
        }
    }

    private static string Subtitle(AccessoryDto accessory) =>
        string.Join(" · ", new[] { accessory.Platform ?? "Any system", accessory.Manufacturer }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    private bool MatchesSearch(AccessoryDto accessory) =>
        string.IsNullOrWhiteSpace(_search)
        || new[] { accessory.Name, accessory.Platform, accessory.Manufacturer, accessory.Model, accessory.Colour, accessory.Notes }
            .Any(text => text?.Contains(_search, StringComparison.OrdinalIgnoreCase) ?? false);

    private void OnUserChanged() => InvokeAsync(async () =>
    {
        await LoadAsync();
        StateHasChanged();
    });

    public void Dispose() => CurrentUser.Changed -= OnUserChanged;
}
