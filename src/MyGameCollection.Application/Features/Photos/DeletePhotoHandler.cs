using MediatR;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Photos;

internal sealed class DeletePhotoHandler(IItemRepository items, IPhotoStorage storage) : IRequestHandler<DeletePhotoCommand>
{
    public async Task Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.ItemId, cancellationToken)
                   ?? throw new NotFoundException("Item", request.ItemId);

        var photo = item.RemovePhoto(request.PhotoId);
        await items.UpdateAsync(item, cancellationToken);

        await storage.DeleteAsync(photo.FileName, cancellationToken);
        await storage.DeleteAsync(photo.ThumbnailFileName, cancellationToken);
    }
}
