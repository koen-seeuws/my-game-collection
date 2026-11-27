using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Commands.AddAddOn;
using MyGameCollection.Application.Abstractions.Commands.UpdateAddOn;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing an add-on of a game (same rules as the domain).</summary>
public sealed class AddOnFormModel : ReleaseDateFormModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public AddOnType Type { get; set; } = AddOnType.Dlc;

    /// <summary>The add-on's IGDB id, set when it is picked from IGDB.</summary>
    public long? CatalogId { get; set; }

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;
    public GameFormat Format { get; set; } = GameFormat.Digital;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public Completeness Completeness { get; set; } = Completeness.CompleteInBox;

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Enter a full http(s) URL."), StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>Only physical add-ons have a box and wear.</summary>
    public bool IsPhysical => Format == GameFormat.Physical;

    public static AddOnFormModel From(AddOnDto addOn) => new()
    {
        Title = addOn.Title,
        Type = addOn.Type,
        CatalogId = addOn.CatalogId,
        Status = addOn.Status,
        Format = addOn.Format,
        Condition = addOn.Condition,
        Completeness = addOn.Completeness,
        ReleaseDate = addOn.ReleaseDate,
        PurchaseDate = addOn.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = addOn.PurchasePrice,
        PurchaseOrigin = addOn.PurchaseOrigin,
        Notes = addOn.Notes,
        ThumbnailUrl = addOn.ThumbnailUrl
    };

    public AddAddOnCommand ToAddCommand(Guid gameKey) => new()
    {
        GameKey = gameKey,
        Title = Title,
        Type = Type,
        CatalogId = CatalogId,
        Status = Status,
        Format = Format,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    public UpdateAddOnCommand ToUpdateCommand(Guid key) => new()
    {
        Key = key,
        Title = Title,
        Type = Type,
        CatalogId = CatalogId,
        Status = Status,
        Format = Format,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    /// <summary>Wishlist items have not been bought yet, so their purchase details are left out.</summary>
    private DateOnly? PurchaseDateOrNull =>
        Status == CollectionStatus.Owned && PurchaseDate is { } date ? DateOnly.FromDateTime(date) : null;

    /// <summary>Add-ons claimed through a subscription are not bought, so they have no price either.</summary>
    private decimal? PurchasePriceOrNull =>
        Status == CollectionStatus.Owned && Format != GameFormat.Subscription ? PurchasePrice : null;

    /// <summary>Only owned physical copies can be bought secondhand; a download is always new.</summary>
    private PurchaseOrigin? PurchaseOriginOrNull =>
        Status == CollectionStatus.Owned && IsPhysical ? PurchaseOrigin : null;
}
