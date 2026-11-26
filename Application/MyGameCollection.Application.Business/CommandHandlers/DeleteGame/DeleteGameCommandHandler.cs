using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.DeleteGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.DeleteGame;

public class DeleteGameCommandHandler : GameCollectionCommandHandler<DeleteGameCommand>
{
    public DeleteGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, DeleteGameCommand command) =>
        collection.DeleteGame(command.Key);
}
