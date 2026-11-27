using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Abstractions.Commands.UpdateConsole;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing a console (same rules as the domain).</summary>
public sealed class ConsoleFormModel : ReleaseDateFormModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Model { get; set; }

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;
    public Region Region { get; set; } = Region.Pal;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public Completeness Completeness { get; set; } = Completeness.Loose;

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Enter a full http(s) URL."), StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public static ConsoleFormModel From(ConsoleDto console) => new()
    {
        Name = console.Name,
        Manufacturer = console.Manufacturer,
        Model = console.Model,
        Status = console.Status,
        Region = console.Region,
        Condition = console.Condition,
        Completeness = console.Completeness,
        ReleaseDate = console.ReleaseDate,
        PurchaseDate = console.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = console.PurchasePrice,
        Notes = console.Notes,
        ThumbnailUrl = console.ThumbnailUrl
    };

    public AddConsoleCommand ToAddCommand() => new()
    {
        Name = Name,
        Manufacturer = Manufacturer,
        Model = Model,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    public UpdateConsoleCommand ToUpdateCommand(Guid key) => new()
    {
        Key = key,
        Name = Name,
        Manufacturer = Manufacturer,
        Model = Model,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    /// <summary>Wishlist items have not been bought yet, so their purchase details are left out.</summary>
    private DateOnly? PurchaseDateOrNull =>
        Status == CollectionStatus.Owned && PurchaseDate is { } date ? DateOnly.FromDateTime(date) : null;

    private decimal? PurchasePriceOrNull => Status == CollectionStatus.Owned ? PurchasePrice : null;
}
