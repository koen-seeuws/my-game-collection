namespace MyGameCollection.Application.Abstractions.ReadModels;

public sealed record PlatformCount(Guid PlatformId, string Platform, int Games, int Consoles);

/// <summary>Aggregated numbers for the dashboard. "Owned" includes items marked for sale.</summary>
public sealed record CollectionStatistics(
    int GameCount,
    int ConsoleCount,
    int WishlistCount,
    int CompleteCount,
    decimal TotalSpent,
    decimal TotalEstimatedValue,
    IReadOnlyList<PlatformCount> TopPlatforms);
