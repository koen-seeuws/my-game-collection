using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyCollectionSummary;

public record CollectionSummaryDto
{
    public required int OwnedGames { get; init; }
    public required int WishlistGames { get; init; }
    public required int OwnedConsoles { get; init; }
    public required int WishlistConsoles { get; init; }

    /// <summary>The sum of the purchase prices of everything owned.</summary>
    public required decimal TotalSpent { get; init; }

    /// <summary>One entry per platform that has games or an owned console, most games first.</summary>
    public required IReadOnlyList<PlatformSummaryDto> Platforms { get; init; }

    /// <summary>How many owned games there are per format (physical, digital, subscription).</summary>
    public required IReadOnlyDictionary<GameFormat, int> OwnedGamesByFormat { get; init; }

    /// <summary>How many owned physical games there are per completeness (digital games have no box).</summary>
    public required IReadOnlyDictionary<Completeness, int> OwnedGamesByCompleteness { get; init; }
}

public record PlatformSummaryDto
{
    public required string Platform { get; init; }
    public required int OwnedGames { get; init; }
    public required int WishlistGames { get; init; }

    /// <summary>Whether an owned console with the same name as the platform is in the collection.</summary>
    public required bool HasConsole { get; init; }
}
