using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AcquireGame;

public class AcquireGameCommandHandler : ICommandHandler<AcquireGameCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public AcquireGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(AcquireGameCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.AcquireGame(command.Key, command.PurchaseDate, command.PurchasePrice);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
