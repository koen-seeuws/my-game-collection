using MediatR;
using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;
using MyGameCollection.Domain.Entities;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Items;

internal sealed class CreateItemHandler(IItemRepository items, IPlatformRepository platforms, ILogger<CreateItemHandler> logger)
    : IRequestHandler<CreateItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var model = request.Item;
        if (model.PlatformId is not { } platformId || !await platforms.ExistsAsync(platformId, cancellationToken))
            throw new ValidationException("Pick an existing platform.");

        CollectionItem item = model.Type == ItemType.Console ? new GameConsole() : new Game();
        model.ApplyTo(item);

        await items.AddAsync(item, cancellationToken);
        logger.LogInformation("Added {Type} '{Title}' ({Id})", item.Type, item.Title, item.Id);
        return item.Id;
    }
}
