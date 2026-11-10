using MediatR;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;

namespace MyGameCollection.Application.Features.Items;

internal sealed class SearchItemsHandler(IItemRepository items) : IRequestHandler<SearchItemsQuery, PagedResult<ItemListDto>>
{
    public Task<PagedResult<ItemListDto>> Handle(SearchItemsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter with
        {
            Page = Math.Max(request.Filter.Page, 0),
            PageSize = Math.Clamp(request.Filter.PageSize, 1, 200)
        };
        return items.SearchAsync(filter, cancellationToken);
    }
}
