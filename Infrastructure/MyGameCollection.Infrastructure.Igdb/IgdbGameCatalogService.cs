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
                Platforms = (game.Platforms ?? [])
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => new CatalogPlatformDto { Name = p.Name!, Abbreviation = p.Abbreviation })
                    .ToList()
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var platforms = await igdb.QueryAsync<IgdbPlatform>("platforms",
            $"search {IgdbClient.Quote(search.Trim())}; " +
            "fields name, abbreviation, alternative_name, platform_logo.image_id, " +
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
                    ReleaseYear = versions
                        .SelectMany(v => v.ReleaseDates ?? [])
                        .Select(d => d.Year)
                        .Where(y => y is > 0)
                        .Min(),
                    LogoUrl = IgdbClient.ImageUrl(platform.PlatformLogo?.ImageId, "t_thumb")
                };
            })
            .ToList();
    }
}
