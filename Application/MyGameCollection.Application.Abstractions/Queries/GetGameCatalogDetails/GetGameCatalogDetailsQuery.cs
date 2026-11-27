namespace MyGameCollection.Application.Abstractions.Queries.GetGameCatalogDetails;

/// <summary>
/// What the external catalog (IGDB) knows of a game right now, beyond what the collection stores: its rating,
/// summary, time to beat and screenshots. These change over time, so they are fetched live (and cached briefly)
/// instead of stored. Empty when the catalog no longer knows the game.
/// </summary>
/// <exception cref="Services.CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
public record GetGameCatalogDetailsQuery : IQuery<GameCatalogDetailsDto?>
{
    /// <summary>The game's id in the catalog, as stored with the game.</summary>
    public required long CatalogId { get; init; }
}
