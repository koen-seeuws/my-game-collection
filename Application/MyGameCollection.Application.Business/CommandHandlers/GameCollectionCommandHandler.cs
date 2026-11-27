using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers;

/// <summary>
/// Shared flow of every command on a collection: load the current user's collection, change it, save it.
/// </summary>
public abstract class GameCollectionCommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    protected GameCollectionCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;

        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        Apply(collection, command);

        await _repository.SaveAsync(collection, cancellationToken);
    }

    /// <summary>Applies the command to the (loaded) collection of the current user.</summary>
    protected abstract void Apply(GameCollection collection, TCommand command);
}
