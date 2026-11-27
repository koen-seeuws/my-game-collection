using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.Igdb;

/// <summary>Who developed and who published a game, from IGDB's involved companies.</summary>
internal static class IgdbCompanies
{
    /// <summary>The fields to ask for on the games endpoint.</summary>
    public const string Fields =
        "involved_companies.company.name, involved_companies.developer, involved_companies.publisher";

    public static IReadOnlyList<string> Developers(IgdbGame game) => Names(game, c => c.Developer);

    public static IReadOnlyList<string> Publishers(IgdbGame game) => Names(game, c => c.Publisher);

    /// <summary>
    /// The names of the companies that played the part, trimmed and without duplicates, in IGDB's order. Capped at
    /// what a game can hold: some games list a publisher for every region.
    /// </summary>
    private static IReadOnlyList<string> Names(IgdbGame game, Func<IgdbInvolvedCompany, bool> part) =>
        (game.InvolvedCompanies ?? [])
            .Where(part)
            .Select(c => c.Company?.Name?.Trim())
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(CollectionItemLimits.MaxCompanies)
            .ToList();
}
