using System.ComponentModel.DataAnnotations;
using MyGameCollection.Application.Abstractions.Contracts;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for adding or editing a game (same rules as the domain).</summary>
public sealed class GameFormModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Platform { get; set; } = string.Empty;

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;
    public Region Region { get; set; } = Region.Pal;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public Completeness Completeness { get; set; } = Completeness.CompleteInBox;

    [Range(1950, 2100)]
    public int? ReleaseYear { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Enter a full http(s) URL."), StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public static GameFormModel From(GameDto game) => new()
    {
        Title = game.Title,
        Platform = game.Platform,
        Status = game.Status,
        Region = game.Region,
        Condition = game.Condition,
        Completeness = game.Completeness,
        ReleaseYear = game.ReleaseYear,
        PurchaseDate = game.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = game.PurchasePrice,
        Notes = game.Notes,
        ThumbnailUrl = game.ThumbnailUrl
    };

    /// <summary>Wishlist items have not been bought yet, so their purchase details are left out.</summary>
    public GameData ToData() => new()
    {
        Title = Title,
        Platform = Platform,
        Status = Status,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseYear = ReleaseYear,
        PurchaseDate = Status == CollectionStatus.Owned && PurchaseDate is { } date ? DateOnly.FromDateTime(date) : null,
        PurchasePrice = Status == CollectionStatus.Owned ? PurchasePrice : null,
        Notes = Notes,
        ThumbnailUrl = ThumbnailUrl
    };
}
