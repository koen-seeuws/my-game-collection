using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Mapping;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AddConsole;

public class AddConsoleCommandHandler : GameCollectionCommandHandler<AddConsoleCommand>
{
    public AddConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, AddConsoleCommand command) =>
        collection.AddConsole(command.Data.ToDomain());
}
