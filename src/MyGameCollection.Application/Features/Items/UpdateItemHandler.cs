using MediatR;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Items;

internal sealed class UpdateItemHandler(IItemRepository items, IPlatformRepository platforms) : IRequestHandler<UpdateItemCommand>
{
    public async Task Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException("Item", request.Id);

        var model = request.Item;
        if (item.Type != model.Type)
            throw new ValidationException("The type of an existing item cannot be changed.");
        if (model.PlatformId is not { } platformId || !await platforms.ExistsAsync(platformId, cancellationToken))
            throw new ValidationException("Pick an existing platform.");

        model.ApplyTo(item);
        await items.UpdateAsync(item, cancellationToken);
    }
}
