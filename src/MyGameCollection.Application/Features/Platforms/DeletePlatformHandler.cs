using MediatR;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Platforms;

internal sealed class DeletePlatformHandler(IPlatformRepository platforms) : IRequestHandler<DeletePlatformCommand>
{
    public async Task Handle(DeletePlatformCommand request, CancellationToken cancellationToken)
    {
        if (!await platforms.ExistsAsync(request.Id, cancellationToken))
            throw new NotFoundException("Platform", request.Id);
        if (await platforms.HasItemsAsync(request.Id, cancellationToken))
            throw new ValidationException("This platform still has items in your collection.");

        await platforms.DeleteAsync(request.Id, cancellationToken);
    }
}
