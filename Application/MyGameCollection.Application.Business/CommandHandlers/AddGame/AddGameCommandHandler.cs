using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AddGame;

public class AddGameCommandHandler : GameCollectionCommandHandler<AddGameCommand>
{
    public AddGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, AddGameCommand command) =>
        collection.AddGame(command.Data.ToDomain());
}
