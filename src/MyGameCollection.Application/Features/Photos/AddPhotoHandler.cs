using MediatR;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Photos;

internal sealed class AddPhotoHandler(IItemRepository items, IPhotoStorage storage) : IRequestHandler<AddPhotoCommand, Guid>
{
    public async Task<Guid> Handle(AddPhotoCommand request, CancellationToken cancellationToken)
    {
        var upload = request.Upload;
        var extension = PhotoRules.ExtensionFor(upload.ContentType)
                        ?? throw new ValidationException($"Unsupported image type '{upload.ContentType}'. Use JPEG, PNG or WebP.");
        if (upload.SizeBytes > PhotoRules.MaxPhotoBytes)
            throw new ValidationException("Photo is too large.");

        var item = await items.GetByIdAsync(request.ItemId, cancellationToken)
                   ?? throw new NotFoundException("Item", request.ItemId);

        var saved = new List<string>(2);
        try
        {
            saved.Add(await storage.SaveAsync(item.Id, upload.Full, extension, cancellationToken));
            saved.Add(await storage.SaveAsync(item.Id, upload.Thumbnail, extension, cancellationToken));

            var photo = item.AddPhoto(saved[0], saved[1], upload.ContentType, upload.OriginalFileName, upload.SizeBytes);
            await items.UpdateAsync(item, cancellationToken);
            return photo.Id;
        }
        catch
        {
            // Don't leave files behind that no record points to.
            foreach (var file in saved)
                await storage.DeleteAsync(file, CancellationToken.None);
            throw;
        }
    }
}
