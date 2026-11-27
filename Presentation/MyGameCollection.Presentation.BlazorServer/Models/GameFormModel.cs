using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing a game (same rules as the domain).</summary>
public sealed class GameFormModel : ReleaseDateFormModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Platform { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Edition { get; set; }

    /// <summary>The game's genres, filled in from IGDB or picked by hand.</summary>
    public List<string> Genres { get; set; } = [];

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;
    public GameFormat Format { get; set; } = GameFormat.Physical;
    public Region Region { get; set; } = Region.Pal;
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

    public static GameFormModel From(GameDto game) => new()
    {
        Title = game.Title,
        Platform = game.Platform,
        Edition = game.Edition,
        Genres = game.Genres.ToList(),
        Status = game.Status,
        Format = game.Format,
        Region = game.Region,
        Condition = game.Condition,
        Completeness = game.Completeness,
        ReleaseDate = game.ReleaseDate,
        PurchaseDate = game.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = game.PurchasePrice,
        PurchaseOrigin = game.PurchaseOrigin,
        Notes = game.Notes,
        ThumbnailUrl = game.ThumbnailUrl
    };

    public AddGameCommand ToAddCommand() => new()
    {
        Title = Title,
        Platform = Platform,
        Edition = Edition,
        Genres = Genres,
        Status = Status,
        Format = Format,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };

    public UpdateGameCommand ToUpdateCommand(Guid key) => new()
    {
        Key = key,
        Title = Title,
        Platform = Platform,
        Edition = Edition,
        Genres = Genres,
        Status = Status,
        Format = Format,
        Region = Region,
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

    /// <summary>Subscription games are not bought, so they have no price either.</summary>
    private decimal? PurchasePriceOrNull =>
        Status == CollectionStatus.Owned && Format != GameFormat.Subscription ? PurchasePrice : null;

    /// <summary>Only owned physical copies can be bought secondhand; a download is always new.</summary>
    private PurchaseOrigin? PurchaseOriginOrNull =>
        Status == CollectionStatus.Owned && IsPhysical ? PurchaseOrigin : null;

    /// <summary>Only physical copies have a box, manual and wear.</summary>
    public bool IsPhysical => Format == GameFormat.Physical;
}
