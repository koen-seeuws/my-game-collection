using MediatR;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Items;

internal sealed class FindItemsByBarcodeHandler(IItemRepository items) : IRequestHandler<FindItemsByBarcodeQuery, IReadOnlyList<ItemListDto>>
{
    public async Task<IReadOnlyList<ItemListDto>> Handle(FindItemsByBarcodeQuery request, CancellationToken cancellationToken)
    {
        var barcode = BarcodeNormalizer.Normalize(request.Barcode);
        if (barcode is null) return [];
        var result = await items.SearchAsync(new ItemFilter { Barcode = barcode, PageSize = 50 }, cancellationToken);
        return result.Items;
    }
}
