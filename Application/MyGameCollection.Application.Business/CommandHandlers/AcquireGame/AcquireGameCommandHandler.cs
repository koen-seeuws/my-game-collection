using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AcquireGame;

public class AcquireGameCommandHandler : GameCollectionCommandHandler<AcquireGameCommand>
{
    public AcquireGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, AcquireGameCommand command) =>
        collection.AcquireGame(command.Key, command.PurchaseDate, command.PurchasePrice);
}
