using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AcquireConsole;

public class AcquireConsoleCommandHandler : ICommandHandler<AcquireConsoleCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public AcquireConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(AcquireConsoleCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.AcquireConsole(command.Key, command.PurchaseDate, command.PurchasePrice);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
