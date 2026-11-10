using MediatR;
using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Items;

internal sealed class DeleteItemHandler(IItemRepository items, IPhotoStorage storage, ILogger<DeleteItemHandler> logger)
    : IRequestHandler<DeleteItemCommand>
{
    public async Task Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        if (!await items.DeleteAsync(request.Id, cancellationToken))
            throw new NotFoundException("Item", request.Id);

        // Files go after the database row, so a failure here leaves orphaned files rather than broken records.
        await storage.DeleteAllForItemAsync(request.Id, cancellationToken);
        logger.LogInformation("Deleted item {Id}", request.Id);
    }
}
