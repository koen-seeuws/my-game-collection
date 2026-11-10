using MediatR;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Photos;

internal sealed class SetPrimaryPhotoHandler(IItemRepository items) : IRequestHandler<SetPrimaryPhotoCommand>
{
    public async Task Handle(SetPrimaryPhotoCommand request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.ItemId, cancellationToken)
                   ?? throw new NotFoundException("Item", request.ItemId);
        item.SetPrimaryPhoto(request.PhotoId);
        await items.UpdateAsync(item, cancellationToken);
    }
}
