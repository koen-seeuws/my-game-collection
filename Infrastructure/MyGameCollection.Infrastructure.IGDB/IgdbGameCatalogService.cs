using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.IGDB;

/// <summary>Searches games and consoles on IGDB.</summary>
internal sealed class IgdbGameCatalogService(IgdbClient igdb, IOptions<IgdbOptions> options) : IGameCatalogService
{
    public async Task<IReadOnlyList<CatalogGameDto>> SearchGamesAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        // version_parent = null leaves out editions (GOTY, Deluxe...) that would otherwise crowd the results, and
        // add-ons are added to their game instead.
        var games = await igdb.QueryAsync<IgdbGame>("games",
            $"search {IgdbClient.Quote(search.Trim())}; fields {GameFields}; " +
            $"where version_parent = null & game_type != {IgdbGameDetails.AddOnTypeIds}; " +
            $"limit {options.Value.SearchLimit};", cancellationToken);

        return games
            .Where(game => !string.IsNullOrWhiteSpace(game.Name))
            .Select(ToCatalogGame)
            .ToList();
    }

    public async Task<CatalogGameDto?> GetGameAsync(long gameId, CancellationToken cancellationToken = default)
    {
        var games = await igdb.QueryAsync<IgdbGame>("games",
            $"fields {GameFields}; where id = {gameId}; limit 1;", cancellationToken);

        return games
            .Where(game => !string.IsNullOrWhiteSpace(game.Name))
            .Select(ToCatalogGame)
            .FirstOrDefault();
    }

    /// <summary>What the game search and lookup ask for of a game.</summary>
    private static readonly string GameFields =
        $"name, first_release_date, {ReleaseDateFields("release_dates")}, cover.image_id, platforms.name, platforms.abbreviation, genres.name, " +
        $"{IgdbPlayerModes.Fields}, {IgdbCompanies.Fields}, {IgdbGameDetails.Fields}";

    private static CatalogGameDto ToCatalogGame(IgdbGame game) => new CatalogGameDto
    {
        Id = game.Id,
        Name = game.Name!,
        ReleaseDate = FirstReleaseDate(game),
        CoverUrl = IgdbClient.ImageUrl(game.Cover?.ImageId, "t_cover_small"),
        ThumbnailUrl = IgdbClient.ImageUrl(game.Cover?.ImageId, "t_cover_big"),
        Platforms = (game.Platforms ?? [])
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .Select(p => new CatalogPlatformDto
            {
                Name = p.Name!,
                Abbreviation = p.Abbreviation,
                Players = IgdbPlayerModes.ForPlatform(game, p.Id),
                Releases = Releases(game, p.Id)
            })
            .ToList(),
        Players = IgdbPlayerModes.ForGame(game),
        Genres = (game.Genres ?? [])
            .Select(g => g.Name?.Trim())
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList(),
        Developers = IgdbCompanies.Developers(game),
        Publishers = IgdbCompanies.Publishers(game),
        Series = IgdbGameDetails.Series(game),
        Type = IgdbGameDetails.Type(game),
        AgeRatings = IgdbGameDetails.AgeRatings(game)
    };

    public async Task<IReadOnlyList<CatalogGameEditionDto>> GetGameEditionsAsync(long gameId, CancellationToken cancellationToken = default)
    {
        // Editions are games of their own on IGDB, pointing to the standard release through version_parent.
        var editions = await igdb.QueryAsync<IgdbGame>("games",
            $"fields name, version_title, first_release_date, {ReleaseDateFields("release_dates")}, cover.image_id; " +
            $"where version_parent = {gameId}; sort first_release_date asc; limit 50;", cancellationToken);

        return editions
            .Select(edition => (edition, title: EditionTitle(edition)))
            .Where(e => e.title is not null)
            .Select(e => new CatalogGameEditionDto
            {
                Id = e.edition.Id,
                Edition = e.title!,
                ReleaseDate = FirstReleaseDate(e.edition),
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

    public async Task<IReadOnlyList<CatalogAddOnDto>> GetGameAddOnsAsync(long gameId, CancellationToken cancellationToken = default)
    {
        var addOns = await igdb.QueryAsync<IgdbGame>("games",
            $"fields {AddOnFields}; where parent_game = {gameId} & game_type = {IgdbGameDetails.AddOnTypeIds}; " +
            "sort first_release_date asc; limit 200;", cancellationToken);

        return ToCatalogAddOns(addOns);
    }

    public async Task<IReadOnlyList<CatalogAddOnDto>> SearchAddOnsAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var addOns = await igdb.QueryAsync<IgdbGame>("games",
            $"search {IgdbClient.Quote(search.Trim())}; fields {AddOnFields}; " +
            $"where game_type = {IgdbGameDetails.AddOnTypeIds}; limit {options.Value.SearchLimit};", cancellationToken);

        return ToCatalogAddOns(addOns);
    }

    /// <summary>What the add-on lookups ask for of an add-on.</summary>
    private static readonly string AddOnFields =
        $"name, game_type, parent_game.name, first_release_date, {ReleaseDateFields("release_dates")}, cover.image_id";

    private static IReadOnlyList<CatalogAddOnDto> ToCatalogAddOns(IEnumerable<IgdbGame> addOns) =>
        addOns
            .Where(addOn => !string.IsNullOrWhiteSpace(addOn.Name))
            .Select(addOn => new CatalogAddOnDto
            {
                Id = addOn.Id,
                Name = IgdbGameDetails.AddOnName(addOn),
                Type = IgdbGameDetails.AddOnType(addOn),
                GameName = addOn.ParentGame?.Name?.Trim(),
                ReleaseDate = FirstReleaseDate(addOn),
                CoverUrl = IgdbClient.ImageUrl(addOn.Cover?.ImageId, "t_cover_small"),
                ThumbnailUrl = IgdbClient.ImageUrl(addOn.Cover?.ImageId, "t_cover_big")
            })
            .ToList();

    public async Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
            return [];

        var platforms = await igdb.QueryAsync<IgdbPlatform>("platforms",
            $"search {IgdbClient.Quote(search.Trim())}; " +
            "fields name, abbreviation, alternative_name, platform_logo.image_id, " +
            "versions.name, versions.platform_logo.image_id, " +
            "versions.companies.manufacturer, versions.companies.company.name, " +
            $"{ReleaseDateFields("versions.platform_version_release_dates")}; " +
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
                    ReleaseDate = versions.Select(FirstReleaseDate).Min(),
                    LogoUrl = IgdbClient.ImageUrl(platform.PlatformLogo?.ImageId, "t_thumb"),
                    ThumbnailUrl = IgdbClient.ImageUrl(platform.PlatformLogo?.ImageId, "t_logo_med"),
                    Versions = versions
                        .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                        .Select(v => new CatalogConsoleVersionDto
                        {
                            Id = v.Id,
                            Name = v.Name!,
                            ReleaseDate = FirstReleaseDate(v),
                            // Not t_thumb: that square crop cuts off wide pictures and logos.
                            LogoUrl = IgdbClient.ImageUrl(v.PlatformLogo?.ImageId, "t_logo_med"),
                            ThumbnailUrl = IgdbClient.ImageUrl(v.PlatformLogo?.ImageId, "t_logo_med")
                        })
                        // Oldest first; versions without a known release date go last.
                        .OrderBy(v => v.ReleaseDate is null)
                        .ThenBy(v => v.ReleaseDate)
                        .ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                };
            })
            .ToList();
    }

    /// <summary>The release date fields to ask for of <paramref name="path"/> (e.g. "release_dates").</summary>
    private static string ReleaseDateFields(string path) =>
        string.Join(", ", new[] { "date", "date_format", "category", "y", "m" }.Select(field => $"{path}.{field}"));

    /// <summary>
    /// When the game first came out (in any region or on any platform), as precisely as IGDB knows it.
    /// Falls back to the year of first_release_date, which has no precision of its own.
    /// </summary>
    private static ReleaseDate? FirstReleaseDate(IgdbGame game) =>
        FirstReleaseDate(game.ReleaseDates)
        ?? (game.FirstReleaseDate is { } seconds ? new ReleaseDate(DateTimeOffset.FromUnixTimeSeconds(seconds).Year) : null);

    /// <summary>When the game came out on the platform <paramref name="platformId"/>, per region, as precisely as known.</summary>
    private static IReadOnlyList<CatalogReleaseDto> Releases(IgdbGame game, long platformId) =>
        (game.ReleaseDates ?? [])
            .Where(release => release.Platform == platformId)
            .Select(release => (region: IgdbGameDetails.Region(release.ReleaseRegion), date: ToReleaseDate(release)))
            .Where(release => release.date is { Problem: null })
            .Select(release => new CatalogReleaseDto { Region = release.region, ReleaseDate = release.date!.Value })
            .ToList();

    /// <summary>When a platform version first came out in any region (regions often differ).</summary>
    private static ReleaseDate? FirstReleaseDate(IgdbPlatformVersion version) => FirstReleaseDate(version.ReleaseDates);

    private static ReleaseDate? FirstReleaseDate(IReadOnlyList<IgdbReleaseDate>? releases) =>
        (releases ?? [])
            .Select(ToReleaseDate)
            .Where(date => date?.Problem is null)
            .Min();

    /// <summary>
    /// A release as precisely as IGDB knows it. Its format (the older category field uses the same values):
    /// 0 = year, month and day; 1 = year and month; 2 = year; 3-6 = a quarter of the year; 7 = to be decided.
    /// </summary>
    private static ReleaseDate? ToReleaseDate(IgdbReleaseDate release)
    {
        if (release.Date is not { } seconds)
            return null;

        var date = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        var year = release.Year ?? date.Year;

        return (release.DateFormat ?? release.Category) switch
        {
            0 => new ReleaseDate(date.Year, date.Month, date.Day),
            1 => new ReleaseDate(year, release.Month ?? date.Month),
            >= 2 and <= 6 => new ReleaseDate(year),
            7 => null,
            // Unknown precision: only trust the year.
            _ => new ReleaseDate(year)
        };
    }
}
