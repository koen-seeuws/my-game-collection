using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.DeleteConsole;

public class DeleteConsoleCommandHandler : GameCollectionCommandHandler<DeleteConsoleCommand>
{
    public DeleteConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
        : base(repository, userContext)
    {
    }

    protected override void Apply(GameCollection collection, DeleteConsoleCommand command) =>
        collection.DeleteConsole(command.Key);
}
