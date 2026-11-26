using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateGame;

public class UpdateGameCommandHandler : GameCollectionCommandHandler<UpdateGameCommand>
{
    public UpdateGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, UpdateGameCommand command) =>
        collection.UpdateGame(command.Key, command.Data.ToDomain());
}
