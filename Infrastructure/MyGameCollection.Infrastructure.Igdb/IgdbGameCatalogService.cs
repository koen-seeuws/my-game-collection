using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;

namespace MyGameCollection.Infrastructure.Igdb;

/// <summary>Searches games and consoles on IGDB.</summary>
internal sealed class IgdbGameCatalogService(IgdbClient igdb, IOptions<IgdbOptions> options) : IGameCatalogService
{
    public async Task<IReadOnlyList<CatalogGameDto>> SearchGamesAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        // version_parent = null leaves out editions (GOTY, Deluxe...) that would otherwise crowd the results.
        var games = await igdb.QueryAsync<IgdbGame>("games",
            $"search {IgdbClient.Quote(search.Trim())}; " +
            "fields name, first_release_date, cover.image_id, platforms.name, platforms.abbreviation; " +
            $"where version_parent = null; limit {options.Value.SearchLimit};", cancellationToken);

        return games
            .Where(game => !string.IsNullOrWhiteSpace(game.Name))
            .Select(game => new CatalogGameDto
            {
                Id = game.Id,
                Name = game.Name!,
                ReleaseYear = game.FirstReleaseDate is { } seconds
                    ? DateTimeOffset.FromUnixTimeSeconds(seconds).Year
                    : null,
                CoverUrl = IgdbClient.ImageUrl(game.Cover?.ImageId, "t_cover_small"),
                ThumbnailUrl = IgdbClient.ImageUrl(game.Cover?.ImageId, "t_cover_big"),
                Platforms = (game.Platforms ?? [])
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => new CatalogPlatformDto { Name = p.Name!, Abbreviation = p.Abbreviation })
                    .ToList()
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CatalogGameEditionDto>> GetGameEditionsAsync(long gameId, CancellationToken cancellationToken = default)
    {
        // Editions are games of their own on IGDB, pointing to the standard release through version_parent.
        var editions = await igdb.QueryAsync<IgdbGame>("games",
            "fields name, version_title, first_release_date, cover.image_id; " +
            $"where version_parent = {gameId}; sort first_release_date asc; limit 50;", cancellationToken);

        return editions
            .Select(edition => (edition, title: EditionTitle(edition)))
            .Where(e => e.title is not null)
            .Select(e => new CatalogGameEditionDto
            {
                Id = e.edition.Id,
                Edition = e.title!,
                ReleaseYear = e.edition.FirstReleaseDate is { } seconds
                    ? DateTimeOffset.FromUnixTimeSeconds(seconds).Year
                    : null,
                CoverUrl = IgdbClient.ImageUrl(e.edition.Cover?.ImageId, "t_cover_small"),
                ThumbnailUrl = IgdbClient.ImageUrl(e.edition.Cover?.ImageId, "t_cover_big")
            })
            .ToList();
    }

    /// <summary>
    /// The edition's own name: IGDB's version_title ("Pre-Udder Edition"), or else whatever follows the
    /// colon in the full name ("Goat Simulator 3: Pre-Udder Edition").
    /// </summary>
    private static string? EditionTitle(IgdbGame edition)
    {
        if (!string.IsNullOrWhiteSpace(edition.VersionTitle))
            return edition.VersionTitle.Trim();

        var colon = edition.Name?.LastIndexOf(':') ?? -1;
        return colon >= 0 && colon < edition.Name!.Length - 1 ? edition.Name[(colon + 1)..].Trim() : edition.Name?.Trim();
    }

    public async Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var platforms = await igdb.QueryAsync<IgdbPlatform>("platforms",
            $"search {IgdbClient.Quote(search.Trim())}; " +
            "fields name, abbreviation, alternative_name, platform_logo.image_id, " +
            "versions.name, versions.platform_logo.image_id, " +
            "versions.companies.manufacturer, versions.companies.company.name, versions.platform_version_release_dates.y; " +
            $"limit {options.Value.SearchLimit};", cancellationToken);

        return platforms
            .Where(platform => !string.IsNullOrWhiteSpace(platform.Name))
            .Select(platform =>
            {
                var versions = platform.Versions ?? [];
                return new CatalogConsoleDto
                {
                    Id = platform.Id,
                    Name = platform.Name!,
                    Abbreviation = platform.Abbreviation,
                    AlternativeName = platform.AlternativeName,
                    Manufacturer = versions
                        .SelectMany(v => v.Companies ?? [])
                        .Where(c => c.Manufacturer && !string.IsNullOrWhiteSpace(c.Company?.Name))
                        .Select(c => c.Company!.Name)
                        .FirstOrDefault(),
                    // The earliest release of any of the platform's versions (models / revisions).
                    ReleaseYear = versions.Select(EarliestYear).Min(),
                    LogoUrl = IgdbClient.ImageUrl(platform.PlatformLogo?.ImageId, "t_thumb"),
                    ThumbnailUrl = IgdbClient.ImageUrl(platform.PlatformLogo?.ImageId, "t_logo_med"),
                    Versions = versions
                        .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                        .Select(v => new CatalogConsoleVersionDto
                        {
                            Id = v.Id,
                            Name = v.Name!,
                            ReleaseYear = EarliestYear(v),
                            // Not t_thumb: that square crop cuts off wide pictures and logos.
                            LogoUrl = IgdbClient.ImageUrl(v.PlatformLogo?.ImageId, "t_logo_med"),
                            ThumbnailUrl = IgdbClient.ImageUrl(v.PlatformLogo?.ImageId, "t_logo_med")
                        })
                        // Oldest first; versions without a known release date go last.
                        .OrderBy(v => v.ReleaseYear ?? int.MaxValue)
                        .ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                };
            })
            .ToList();
    }

    /// <summary>The earliest year a platform version was released in any region (regions often differ).</summary>
    private static int? EarliestYear(IgdbPlatformVersion version) =>
        (version.ReleaseDates ?? [])
            .Select(d => d.Year)
            .Where(y => y is > 0)
            .Min();
}
