using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.SearchConsoleCatalog;
using MyGameCollection.Infrastructure.GameCatalog.Igdb;
using Microsoft.Extensions.Options;

namespace MyGameCollection.Infrastructure.GameCatalog.QueryHandlers;

internal sealed class SearchConsoleCatalogQueryHandler(IgdbClient igdb, IOptions<IgdbOptions> options)
    : IQueryHandler<SearchConsoleCatalogQuery, IReadOnlyList<CatalogConsoleDto>>
{
    public async Task<IReadOnlyList<CatalogConsoleDto>> HandleAsync(SearchConsoleCatalogQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Search))
            return [];

        var platforms = await igdb.QueryAsync<IgdbPlatform>("platforms",
            $"search {IgdbClient.Quote(query.Search.Trim())}; " +
            "fields name, abbreviation, alternative_name, platform_logo.image_id, " +
            "versions.companies.manufacturer, versions.companies.company.name, versions.platform_version_release_dates.y; " +
            $"limit {options.Value.SearchLimit};", ct);

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
