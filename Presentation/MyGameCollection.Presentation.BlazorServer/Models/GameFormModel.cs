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

    /// <summary>The studios that made the game, filled in from IGDB or typed in.</summary>
    public List<string> Developers { get; set; } = [];

    /// <summary>The companies that published the game, filled in from IGDB or typed in.</summary>
    public List<string> Publishers { get; set; } = [];

    /// <summary>The series or franchise, filled in from IGDB or typed in.</summary>
    [StringLength(100)]
    public string? Series { get; set; }

    /// <summary>A game of its own, a remake, a bundle... Empty when unknown.</summary>
    public GameType? Type { get; set; }

    /// <summary>The age rating on the box, e.g. "PEGI 7"; filled in from IGDB for the chosen region.</summary>
    [StringLength(100)]
    public string? AgeRating { get; set; }

    /// <summary>The game's IGDB id, set when it is picked from IGDB.</summary>
    public long? CatalogId { get; set; }

    public CollectionStatus Status { get; set; } = CollectionStatus.Owned;

    // ---- Players ----

    public bool SinglePlayer { get; set; }
    public bool Multiplayer { get; set; }

    [Range(CollectionItemLimits.MinMultiplayerPlayers, CollectionItemLimits.MaxPlayers,
        ErrorMessage = "Enter between {1} and {2} players, or leave it empty.")]
    public int? MaxLocalPlayers { get; set; }

    [Range(CollectionItemLimits.MinMultiplayerPlayers, CollectionItemLimits.MaxPlayers,
        ErrorMessage = "Enter between {1} and {2} players, or leave it empty.")]
    public int? MaxOnlinePlayers { get; set; }

    public bool CouchCoop { get; set; }
    public bool OnlineCoop { get; set; }
    public bool SplitScreen { get; set; }

    /// <summary>The player fields as one value; the multiplayer details only count while multiplayer is ticked.</summary>
    public PlayerModes? Players
    {
        get => new PlayerModes
        {
            SinglePlayer = SinglePlayer,
            Multiplayer = Multiplayer,
            MaxLocalPlayers = Multiplayer ? MaxLocalPlayers : null,
            MaxOnlinePlayers = Multiplayer ? MaxOnlinePlayers : null,
            CouchCoop = Multiplayer && CouchCoop,
            OnlineCoop = Multiplayer && OnlineCoop,
            SplitScreen = Multiplayer && SplitScreen
        }.Normalised();
        set
        {
            SinglePlayer = value?.SinglePlayer ?? false;
            Multiplayer = value?.Multiplayer ?? false;
            MaxLocalPlayers = value?.MaxLocalPlayers;
            MaxOnlinePlayers = value?.MaxOnlinePlayers;
            CouchCoop = value?.CouchCoop ?? false;
            OnlineCoop = value?.OnlineCoop ?? false;
            SplitScreen = value?.SplitScreen ?? false;
        }
    }

    public GameFormat Format { get; set; } = GameFormat.Physical;
    public Region Region { get; set; } = Region.Pal;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public Completeness Completeness { get; set; } = Completeness.CompleteInBox;

    public DateTime? PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; set; }

    /// <summary>How far the owner got with playing the game; empty when not tracked.</summary>
    public PlayStatus? PlayStatus { get; set; }

    /// <summary>When the game was completed; only kept while the play status says it was finished.</summary>
    public DateTime? CompletedOn { get; set; }

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
        Developers = game.Developers.ToList(),
        Publishers = game.Publishers.ToList(),
        Series = game.Series,
        Type = game.Type,
        AgeRating = game.AgeRating,
        CatalogId = game.CatalogId,
        Status = game.Status,
        Format = game.Format,
        Players = game.Players,
        Region = game.Region,
        Condition = game.Condition,
        Completeness = game.Completeness,
        ReleaseDate = game.ReleaseDate,
        PurchaseDate = game.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
        PurchasePrice = game.PurchasePrice,
        PurchaseOrigin = game.PurchaseOrigin,
        PlayStatus = game.PlayStatus,
        CompletedOn = game.CompletedOn?.ToDateTime(TimeOnly.MinValue),
        Notes = game.Notes,
        ThumbnailUrl = game.ThumbnailUrl
    };

    public AddGameCommand ToAddCommand() => new()
    {
        Title = Title,
        Platform = Platform,
        Edition = Edition,
        Genres = Genres,
        Developers = Developers,
        Publishers = Publishers,
        Series = Series,
        Type = Type,
        AgeRating = AgeRating,
        CatalogId = CatalogId,
        Status = Status,
        Format = Format,
        Players = Players,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        PlayStatus = PlayStatus,
        CompletedOn = CompletedOnOrNull,
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
        Developers = Developers,
        Publishers = Publishers,
        Series = Series,
        Type = Type,
        AgeRating = AgeRating,
        CatalogId = CatalogId,
        Status = Status,
        Format = Format,
        Players = Players,
        Region = Region,
        Condition = Condition,
        Completeness = Completeness,
        ReleaseDate = ReleaseDate,
        PurchaseDate = PurchaseDateOrNull,
        PurchasePrice = PurchasePriceOrNull,
        PurchaseOrigin = PurchaseOriginOrNull,
        PlayStatus = PlayStatus,
        CompletedOn = CompletedOnOrNull,
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

    /// <summary>Only a game that was played to the end has a completion date.</summary>
    private DateOnly? CompletedOnOrNull =>
        PlayStatus.IsFinished() && CompletedOn is { } date ? DateOnly.FromDateTime(date) : null;

    /// <summary>Only physical copies have a box, manual and wear.</summary>
    public bool IsPhysical => Format == GameFormat.Physical;
}
