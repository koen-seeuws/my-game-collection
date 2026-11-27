using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.DeleteConsole;

public class DeleteConsoleCommandHandler : ICommandHandler<DeleteConsoleCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public DeleteConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(DeleteConsoleCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.DeleteConsole(command.Key);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
