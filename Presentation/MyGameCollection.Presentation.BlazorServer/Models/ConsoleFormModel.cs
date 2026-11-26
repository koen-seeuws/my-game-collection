using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Contracts;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing a console (same rules as the domain).</summary>
public sealed class ConsoleFormModel
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

    [Range(1950, 2100)]
    public int? ReleaseYear { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public static ConsoleFormModel From(ConsoleDto console) => new()
    {
        Name = console.Name,
        Manufacturer = console.Manufacturer,
        Model = console.Model,
        Status = console.Status,
        Region = console.Region,
        Condition = console.Condition,
        Completeness = console.Completeness,
        ReleaseYear = console.ReleaseYear,
        PurchaseDate = console.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = console.PurchasePrice,
        Notes = console.Notes
    };

    /// <summary>Wishlist items have not been bought yet, so their purchase details are left out.</summary>
    public ConsoleData ToData() => new()
    {
        Name = Name,
        Manufacturer = Manufacturer,
        Model = Model,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseYear = ReleaseYear,
        PurchaseDate = Status == CollectionStatus.Owned && PurchaseDate is { } date ? DateOnly.FromDateTime(date) : null,
        PurchasePrice = Status == CollectionStatus.Owned ? PurchasePrice : null,
        Notes = Notes
    };
}
