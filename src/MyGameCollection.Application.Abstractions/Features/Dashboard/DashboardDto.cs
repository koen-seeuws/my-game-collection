namespace MyGameCollection.Application.Abstractions.Features.Dashboard;

public sealed record DashboardDto(CollectionStatistics Statistics, IReadOnlyList<ItemListDto> RecentlyAdded);
