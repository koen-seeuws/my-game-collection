using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateConsole;

public class UpdateConsoleCommandHandler : GameCollectionCommandHandler<UpdateConsoleCommand>
{
    public UpdateConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, UpdateConsoleCommand command) =>
        collection.UpdateConsole(command.Key, command.Data.ToDomain());
}
