using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.DeleteGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.DeleteGame;

public class DeleteGameCommandHandler : ICommandHandler<DeleteGameCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public DeleteGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(DeleteGameCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.DeleteGame(command.Key);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
