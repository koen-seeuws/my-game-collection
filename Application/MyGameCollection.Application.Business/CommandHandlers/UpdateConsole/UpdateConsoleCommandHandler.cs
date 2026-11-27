using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateConsole;

public class UpdateConsoleCommandHandler : ICommandHandler<UpdateConsoleCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public UpdateConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(UpdateConsoleCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.UpdateConsole(command.Key, command.Data.ToDomain());

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
