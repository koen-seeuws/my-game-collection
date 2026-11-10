using MediatR;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Items;

internal sealed class GetItemForEditHandler(IItemRepository items) : IRequestHandler<GetItemForEditQuery, ItemEditModel>
{
    public async Task<ItemEditModel> Handle(GetItemForEditQuery request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException("Item", request.Id);
        return item.ToEditModel();
    }
}
