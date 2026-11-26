using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.SearchGameCatalog;
using MyGameCollection.Infrastructure.GameCatalog.Igdb;
using Microsoft.Extensions.Options;

namespace MyGameCollection.Infrastructure.GameCatalog.QueryHandlers;

internal sealed class SearchGameCatalogQueryHandler(IgdbClient igdb, IOptions<IgdbOptions> options)
    : IQueryHandler<SearchGameCatalogQuery, IReadOnlyList<CatalogGameDto>>
{
    public async Task<IReadOnlyList<CatalogGameDto>> HandleAsync(SearchGameCatalogQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Search))
            return [];

        // version_parent = null leaves out editions (GOTY, Deluxe...) that would otherwise crowd the results.
        var games = await igdb.QueryAsync<IgdbGame>("games",
            $"search {IgdbClient.Quote(query.Search.Trim())}; " +
            "fields name, first_release_date, cover.image_id, platforms.name, platforms.abbreviation; " +
            $"where version_parent = null; limit {options.Value.SearchLimit};", ct);

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
}
