namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>An external catalog of games and consoles (IGDB), used to prefill the add game / console forms.</summary>
public interface IGameCatalogService
{
    /// <summary>Games whose title matches <paramref name="search"/>; add-ons (DLC, expansions...) are left out.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogGameDto>> SearchGamesAsync(string search, CancellationToken cancellationToken = default);

    /// <summary>The catalog game <paramref name="gameId"/>, to refresh a game picked from the catalog before; empty when it no longer exists.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<CatalogGameDto?> GetGameAsync(long gameId, CancellationToken cancellationToken = default);

    /// <summary>The editions (GOTY, Deluxe...) of the catalog game <paramref name="gameId"/>, oldest first.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogGameEditionDto>> GetGameEditionsAsync(long gameId, CancellationToken cancellationToken = default);

    /// <summary>The add-ons (DLC, expansions, packs) of the catalog game <paramref name="gameId"/>, oldest first.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogAddOnDto>> GetGameAddOnsAsync(long gameId, CancellationToken cancellationToken = default);

    /// <summary>Add-ons (DLC, expansions, packs) of any game whose title matches <paramref name="search"/>.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogAddOnDto>> SearchAddOnsAsync(string search, CancellationToken cancellationToken = default);

    /// <summary>Consoles (platforms) whose name matches <paramref name="search"/>.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default);

    /// <summary>The catalog console (platform) <paramref name="consoleId"/>; empty when it does not exist.</summary>
    /// <exception cref="CatalogUnavailableException">The catalog is not configured or could not be reached.</exception>
    Task<CatalogConsoleDto?> GetConsoleAsync(long consoleId, CancellationToken cancellationToken = default);
}
