using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Commands.UpdateGame;

public record UpdateGameCommand : ICommand
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
    public GameFormat Format { get; init; }

    /// <summary>Single-player, couch or online multiplayer, and how many can play. Empty when unknown.</summary>
    public PlayerModes? Players { get; init; }

    public Region Region { get; init; }
    public ItemCondition Condition { get; init; }
    public Completeness Completeness { get; init; }
    public CollectionStatus Status { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; init; }
    public string? Notes { get; init; }

    /// <summary>Absolute http(s) URL of a cover art or picture shown on the item's card.</summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>The item's id in the external catalog (IGDB). Empty when not picked from the catalog.</summary>
    public long? CatalogId { get; init; }
}
