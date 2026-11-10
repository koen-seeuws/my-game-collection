using MediatR;
using MyGameCollection.Application.Abstractions.Features.Dashboard;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Dashboard;

internal sealed class GetDashboardHandler(IItemRepository items) : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var statistics = await items.GetStatisticsAsync(cancellationToken);
        var recent = await items.SearchAsync(new ItemFilter
        {
            SortBy = ItemSortField.CreatedAt,
            Descending = true,
            PageSize = 8,
            Statuses = [ItemStatus.Owned, ItemStatus.ForSale]
        }, cancellationToken);

        return new DashboardDto(statistics, recent.Items);
    }
}
