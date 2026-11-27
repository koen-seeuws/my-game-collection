using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateAddOn;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateAddOn;

public class UpdateAddOnCommandHandler : ICommandHandler<UpdateAddOnCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public UpdateAddOnCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(UpdateAddOnCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.UpdateAddOn(
            command.Key,
            title: command.Title,
            type: command.Type,
            format: command.Format,
            condition: command.Condition,
            completeness: command.Completeness,
            status: command.Status,
            releaseDate: command.ReleaseDate,
            purchaseDate: command.PurchaseDate,
            purchasePrice: command.PurchasePrice,
            purchaseOrigin: command.PurchaseOrigin,
            notes: command.Notes,
            thumbnailUrl: command.ThumbnailUrl,
            catalogId: command.CatalogId);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
