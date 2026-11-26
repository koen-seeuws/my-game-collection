using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AcquireConsole;

public class AcquireConsoleCommandHandler : GameCollectionCommandHandler<AcquireConsoleCommand>
{
    public AcquireConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, AcquireConsoleCommand command) =>
        collection.AcquireConsole(command.Key, command.PurchaseDate, command.PurchasePrice);
}
