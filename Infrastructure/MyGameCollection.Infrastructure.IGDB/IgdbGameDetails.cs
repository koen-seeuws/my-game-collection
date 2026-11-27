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

    /// <summary>IGDB's game type ids (see its game_types endpoint), grouped into the ones a collection tells apart.</summary>
    public static GameType? Type(IgdbGame game) => game.GameType switch
    {
        null => null,
        0 => GameType.MainGame,
        1 or 13 => GameType.Dlc, // DLC, Pack / Addon
        2 or 4 => GameType.Expansion, // Expansion, Standalone Expansion
        3 => GameType.Bundle,
        8 => GameType.Remake,
        9 => GameType.Remaster,
        10 => GameType.ExpandedGame,
        11 => GameType.Port,
        _ => GameType.Other // Mod, Episode, Season, Fork, Update
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
