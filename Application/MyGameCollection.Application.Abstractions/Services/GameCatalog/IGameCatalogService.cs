namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>An external catalog of games and consoles (IGDB), used to prefill the add game / console forms.</summary>
public interface IGameCatalogService
{
    /// <summary>Games whose title matches <paramref name="search"/>.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogGameDto>> SearchGamesAsync(string search, CancellationToken cancellationToken = default);

    /// <summary>The editions (GOTY, Deluxe...) of the catalog game <paramref name="gameId"/>, oldest first.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogGameEditionDto>> GetGameEditionsAsync(long gameId, CancellationToken cancellationToken = default);

    /// <summary>Consoles (platforms) whose name matches <paramref name="search"/>.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default);
}
