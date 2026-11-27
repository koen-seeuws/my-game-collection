using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetGameCatalogDetails;

namespace MyGameCollection.Infrastructure.IGDB.QueryHandlers.GetGameCatalogDetails;

/// <summary>
/// Fetches a game's rating, summary, time to beat and screenshots from IGDB, and keeps them for
/// <see cref="IgdbOptions.DetailsCacheDuration"/> so opening the same game again does not ask IGDB again.
/// </summary>
internal sealed class GetGameCatalogDetailsQueryHandler(IgdbClient igdb, IMemoryCache cache, IOptions<IgdbOptions> options)
    : IQueryHandler<GetGameCatalogDetailsQuery, GameCatalogDetailsDto?>
{
    /// <summary>Enough to get an idea of the game, without loading dozens of pictures.</summary>
    private const int MaxScreenshots = 8;

    public async Task<GameCatalogDetailsDto?> HandleAsync(GetGameCatalogDetailsQuery query, CancellationToken cancellationToken)
    {
        // Failures throw and are not cached, so the next opening tries again.
        return await cache.GetOrCreateAsync(CacheKey(query.CatalogId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = options.Value.DetailsCacheDuration;
            return await FetchAsync(query.CatalogId, cancellationToken);
        });
    }

    internal static string CacheKey(long catalogId) => $"igdb:game-details:{catalogId}";

    private async Task<GameCatalogDetailsDto?> FetchAsync(long catalogId, CancellationToken cancellationToken)
    {
        var gameTask = igdb.QueryAsync<IgdbGameLiveDetails>("games",
            $"fields summary, total_rating, total_rating_count, screenshots.image_id, url; where id = {catalogId}; limit 1;",
            cancellationToken);
        var timeToBeatTask = igdb.QueryAsync<IgdbTimeToBeat>("game_time_to_beats",
            $"fields hastily, normally, completely, count; where game_id = {catalogId}; limit 1;", cancellationToken);

        var game = (await gameTask).FirstOrDefault();
        var timeToBeat = (await timeToBeatTask).FirstOrDefault();

        return game is null ? null : ToDetails(game, timeToBeat);
    }

    internal static GameCatalogDetailsDto ToDetails(IgdbGameLiveDetails game, IgdbTimeToBeat? timeToBeat) => new()
    {
        Summary = string.IsNullOrWhiteSpace(game.Summary) ? null : game.Summary.Trim(),
        // IGDB gives a rating even when only a handful voted; without votes there is nothing to show.
        Rating = game.TotalRatingCount > 0 ? game.TotalRating : null,
        RatingCount = game.TotalRatingCount ?? 0,
        TimeToBeat = ToTimeToBeat(timeToBeat),
        Screenshots = (game.Screenshots ?? [])
            .Select(s => s.ImageId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Take(MaxScreenshots)
            .Select(id => new ScreenshotDto
            {
                ThumbnailUrl = IgdbClient.ImageUrl(id, "t_screenshot_med")!,
                Url = IgdbClient.ImageUrl(id, "t_1080p")!
            })
            .ToList(),
        CatalogUrl = game.Url
    };

    private static TimeToBeatDto? ToTimeToBeat(IgdbTimeToBeat? time)
    {
        if (time is null)
            return null;

        var result = new TimeToBeatDto
        {
            Rushed = Duration(time.Hastily),
            Normal = Duration(time.Normally),
            Completionist = Duration(time.Completely),
            Count = time.Count ?? 0
        };

        return result is { Rushed: null, Normal: null, Completionist: null } ? null : result;
    }

    private static TimeSpan? Duration(long? seconds) => seconds > 0 ? TimeSpan.FromSeconds(seconds.Value) : null;
}
