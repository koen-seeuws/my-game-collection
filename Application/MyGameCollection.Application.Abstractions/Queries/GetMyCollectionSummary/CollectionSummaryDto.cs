using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Queries.GetMyCollectionSummary;

public record CollectionSummaryDto
{
    public required int OwnedGames { get; init; }
    public required int WishlistGames { get; init; }
    public required int OwnedConsoles { get; init; }
    public required int WishlistConsoles { get; init; }

    /// <summary>How many add-ons (DLC, expansions...) of games are owned, and how many wished for.</summary>
    public required int OwnedAddOns { get; init; }
    public required int WishlistAddOns { get; init; }

    /// <summary>How many accessories are owned (counting each one of a quantity), and how many entries are wished for.</summary>
    public required int OwnedAccessories { get; init; }
    public required int WishlistAccessories { get; init; }

    /// <summary>The sum of the purchase prices of everything owned.</summary>
    public required decimal TotalSpent { get; init; }

    /// <summary>One entry per platform that has games or an owned console, most games first.</summary>
    public required IReadOnlyList<PlatformSummaryDto> Platforms { get; init; }

    /// <summary>How many owned games there are per format (physical, digital, subscription).</summary>
    public required IReadOnlyDictionary<GameFormat, int> OwnedGamesByFormat { get; init; }

    /// <summary>How many owned physical games there are per completeness (digital games have no box).</summary>
    public required IReadOnlyDictionary<Completeness, int> OwnedGamesByCompleteness { get; init; }

    /// <summary>How many owned games there are per play status; games whose play status is not tracked are left out.</summary>
    public required IReadOnlyDictionary<PlayStatus, int> OwnedGamesByPlayStatus { get; init; }
}

public record PlatformSummaryDto
{
    public required string Platform { get; init; }
    public required int OwnedGames { get; init; }
    public required int WishlistGames { get; init; }

    /// <summary>Whether an owned console with the same name as the platform is in the collection.</summary>
    public required bool HasConsole { get; init; }
}
