using MediatR;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Application.Features.Photos;

internal sealed class GetPhotoFileHandler(IItemRepository items, IPhotoStorage storage) : IRequestHandler<GetPhotoFileQuery, PhotoFile?>
{
    public async Task<PhotoFile?> Handle(GetPhotoFileQuery request, CancellationToken cancellationToken)
    {
        var photo = await items.GetPhotoAsync(request.PhotoId, cancellationToken);
        if (photo is null) return null;

        var stream = storage.OpenRead(request.Thumbnail ? photo.ThumbnailFileName : photo.FileName);
        return stream is null ? null : new PhotoFile(stream, photo.ContentType, photo.CreatedAt);
    }
}
