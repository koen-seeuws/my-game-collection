using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireAddOn;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AcquireAddOn;

public class AcquireAddOnCommandHandler : ICommandHandler<AcquireAddOnCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public AcquireAddOnCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(AcquireAddOnCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.AcquireAddOn(command.Key, command.PurchaseDate, command.PurchasePrice, command.PurchaseOrigin);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
