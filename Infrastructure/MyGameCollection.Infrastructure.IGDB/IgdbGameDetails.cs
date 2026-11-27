using MyGameCollection.Application.Abstractions.Services.GameCatalog;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.IGDB;

/// <summary>A game's series, type and age ratings from IGDB, and where its releases came out.</summary>
internal static class IgdbGameDetails
{
    /// <summary>The fields to ask for on the games endpoint.</summary>
    public const string Fields =
        "game_type, collections.name, franchise.name, franchises.name, " +
        "age_ratings.organization.name, age_ratings.rating_category.rating, " +
        "release_dates.platform, release_dates.release_region.region";

    /// <summary>
    /// The game's series: its first collection ("Super Mario"), or else its franchise ("Mario"). Collections are the
    /// narrower of the two, so they tell series apart that share a franchise (Mario Kart, Mario Party...).
    /// </summary>
    public static string? Series(IgdbGame game) =>
        new[] { game.Collections?.FirstOrDefault()?.Name, game.Franchise?.Name, game.Franchises?.FirstOrDefault()?.Name }
            .Select(name => name?.Trim())
            .FirstOrDefault(name => !string.IsNullOrEmpty(name));

    /// <summary>IGDB's game type ids of add-ons, which need their game to play: DLC, Expansion and Pack / Addon.</summary>
    public const string AddOnTypeIds = "(1,2,13)";

    /// <summary>
    /// IGDB's game type ids (see its game_types endpoint), grouped into the ones a collection tells apart. Empty for
    /// add-ons, which are no games of their own (see <see cref="AddOnType"/>).
    /// </summary>
    public static GameType? Type(IgdbGame game) => game.GameType switch
    {
        null or 1 or 2 or 13 => null,
        0 => GameType.MainGame,
        4 => GameType.Expansion, // Standalone Expansion
        3 => GameType.Bundle,
        8 => GameType.Remake,
        9 => GameType.Remaster,
        10 => GameType.ExpandedGame,
        11 => GameType.Port,
        _ => GameType.Other // Mod, Episode, Season, Fork, Update
    };

    /// <summary>
    /// An add-on's own name, without its game's name in front: "Kraken Ship Pack" rather than
    /// "Assassin's Creed IV Black Flag: Kraken Ship Pack", since it is always shown with its game.
    /// </summary>
    public static string AddOnName(IgdbGame addOn)
    {
        var name = addOn.Name?.Trim() ?? string.Empty;
        var gameName = addOn.ParentGame?.Name?.Trim();
        if (string.IsNullOrEmpty(gameName) || !name.StartsWith(gameName, StringComparison.OrdinalIgnoreCase))
            return name;

        var rest = name[gameName.Length..].TrimStart(' ', ':', '-', '–', '—');
        return rest.Length > 0 && rest.Length < name.Length - gameName.Length ? rest : name;
    }

    /// <summary>What kind of add-on an IGDB add-on (see <see cref="AddOnTypeIds"/>) is.</summary>
    public static AddOnType AddOnType(IgdbGame game) => game.GameType switch
    {
        1 => Core.Domain.Abstractions.AddOnType.Dlc,
        2 => Core.Domain.Abstractions.AddOnType.Expansion,
        13 => Core.Domain.Abstractions.AddOnType.Pack,
        _ => Core.Domain.Abstractions.AddOnType.Other
    };

    public static IReadOnlyList<CatalogAgeRatingDto> AgeRatings(IgdbGame game) =>
        (game.AgeRatings ?? [])
            .Select(r => (organization: Organization(r.Organization?.Name), rating: r.RatingCategory?.Rating?.Trim()))
            .Where(r => r.organization is not null && !string.IsNullOrEmpty(r.rating))
            .Select(r => new CatalogAgeRatingDto { Organization = r.organization!, Rating = r.rating! })
            .DistinctBy(r => r.Organization)
            .ToList();

    /// <summary>The rating board as printed on boxes; IGDB writes Brazil's as "CLASS_IND".</summary>
    private static string? Organization(string? name) =>
        name?.Trim() switch
        {
            null or "" => null,
            "CLASS_IND" => "ClassInd",
            var other => other
        };

    /// <summary>
    /// The region a release came out in, by video standard: Europe and Oceania are PAL, the Americas NTSC-U and
    /// Japan NTSC-J. Empty for worldwide releases, which count for every region.
    /// </summary>
    public static Region? Region(IgdbReleaseRegion? region) => region?.Region switch
    {
        null or "worldwide" => null,
        "europe" or "australia" or "new_zealand" => Core.Domain.Abstractions.Region.Pal,
        "north_america" or "brazil" => Core.Domain.Abstractions.Region.NtscU,
        "japan" => Core.Domain.Abstractions.Region.NtscJ,
        _ => Core.Domain.Abstractions.Region.Other // china, asia, korea
    };
}
