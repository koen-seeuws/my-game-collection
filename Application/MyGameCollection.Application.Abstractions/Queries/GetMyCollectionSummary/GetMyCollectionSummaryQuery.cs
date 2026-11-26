namespace MyGameCollection.Application.Abstractions.Queries.GetMyCollectionSummary;

/// <summary>Key figures about the current user's collection, for the dashboard.</summary>
public record GetMyCollectionSummaryQuery : IQuery<CollectionSummaryDto>
{
}
