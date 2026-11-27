using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Commands.AddAccessory;
using MyGameCollection.Application.Abstractions.Commands.UpdateAccessory;
using MyGameCollection.Application.Abstractions.Queries.GetMyAccessories;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing an accessory (same rules as the domain).</summary>
public sealed class AccessoryFormModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public AccessoryType Type { get; set; } = AccessoryType.Controller;

    /// <summary>The system it is for; empty when it works with anything.</summary>
    [StringLength(100)]
    public string? Platform { get; set; }

    [StringLength(100)]
    public string? Manufacturer { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    /// <summary>Colour or special edition, e.g. "Ocean Blue".</summary>
    [StringLength(100)]
    public string? Colour { get; set; }

    [Range(1, CollectionItemLimits.MaxAccessoryQuantity)]
    public int Quantity { get; set; } = 1;

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;
    public Region Region { get; set; } = Region.Pal;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public Completeness Completeness { get; set; } = Completeness.Loose;

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Enter a full http(s) URL."), StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public static AccessoryFormModel From(AccessoryDto accessory) => new()
    {
        Name = accessory.Name,
        Type = accessory.Type,
        Platform = accessory.Platform,
        Manufacturer = accessory.Manufacturer,
        Model = accessory.Model,
        Colour = accessory.Colour,
        Quantity = accessory.Quantity,
        Status = accessory.Status,
        Region = accessory.Region,
        Condition = accessory.Condition,
        Completeness = accessory.Completeness,
        PurchaseDate = accessory.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = accessory.PurchasePrice,
        PurchaseOrigin = accessory.PurchaseOrigin,
        Notes = accessory.Notes,
        ThumbnailUrl = accessory.ThumbnailUrl
    };

    public AddAccessoryCommand ToAddCommand() => new()
    {
        Name = Name,
        Type = Type,
        Platform = Platform,
        Manufacturer = Manufacturer,
        Model = Model,
        Colour = Colour,
        Quantity = Quantity,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    public UpdateAccessoryCommand ToUpdateCommand(Guid key) => new()
    {
        Key = key,
        Name = Name,
        Type = Type,
        Platform = Platform,
        Manufacturer = Manufacturer,
        Model = Model,
        Colour = Colour,
        Quantity = Quantity,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    /// <summary>Wishlist items have not been bought yet, so their purchase details are left out.</summary>
    private DateOnly? PurchaseDateOrNull =>
        Status == CollectionStatus.Owned && PurchaseDate is { } date ? DateOnly.FromDateTime(date) : null;

    private decimal? PurchasePriceOrNull => Status == CollectionStatus.Owned ? PurchasePrice : null;

    private PurchaseOrigin? PurchaseOriginOrNull => Status == CollectionStatus.Owned ? PurchaseOrigin : null;
}
