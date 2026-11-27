using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyGames;

public record GameDto
{
    public required Guid Key { get; init; }
    public required string Title { get; init; }
    public required string Platform { get; init; }

    /// <summary>The edition, e.g. "Game of the Year Edition". Empty for the standard release.</summary>
    public string? Edition { get; init; }

    /// <summary>The game's genres, e.g. "Platform". Empty when unknown.</summary>
    public IReadOnlyList<string> Genres { get; init; } = [];

    /// <summary>The studios that made the game, e.g. "Nintendo EPD". Empty when unknown.</summary>
    public IReadOnlyList<string> Developers { get; init; } = [];

    /// <summary>The companies that brought the game out, e.g. "Nintendo". Empty when unknown.</summary>
    public IReadOnlyList<string> Publishers { get; init; } = [];

    /// <summary>The series or franchise the game belongs to, e.g. "Super Mario". Empty when unknown.</summary>
    public string? Series { get; init; }

    /// <summary>A game of its own, a remake, a bundle... Empty when unknown.</summary>
    public GameType? Type { get; init; }

    /// <summary>The age rating on the box for the game's region, e.g. "PEGI 7". Empty when unknown.</summary>
    public string? AgeRating { get; init; }

    /// <summary>Physical copy, bought download or subscription (e.g. PS Plus).</summary>
    public required GameFormat Format { get; init; }

    /// <summary>Single-player, couch or online multiplayer, and how many can play. Empty when unknown.</summary>
    public PlayerModes? Players { get; init; }

    public required Region Region { get; init; }
    public required ItemCondition Condition { get; init; }
    public required Completeness Completeness { get; init; }
    public required CollectionStatus Status { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }

    /// <summary>How far the owner got with playing the game. Empty when not tracked.</summary>
    public PlayStatus? PlayStatus { get; init; }

    /// <summary>When the game was completed (or 100%'d); empty when unknown.</summary>
    public DateOnly? CompletedOn { get; init; }

    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>The item's id in the external catalog (IGDB). Empty when not picked from the catalog.</summary>
    public long? CatalogId { get; init; }

    /// <summary>The DLC, expansions... owned or wished for this game, in the order they were added.</summary>
    public IReadOnlyList<AddOnDto> AddOns { get; init; } = [];
}
