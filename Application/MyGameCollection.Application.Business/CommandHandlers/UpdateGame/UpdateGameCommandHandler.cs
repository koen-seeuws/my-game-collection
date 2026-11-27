using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateGame;

public class UpdateGameCommandHandler : ICommandHandler<UpdateGameCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public UpdateGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.UpdateGame(command.Key, command.Data.ToDomain());

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
