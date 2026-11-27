using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AddConsole;

public class AddConsoleCommandHandler : ICommandHandler<AddConsoleCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public AddConsoleCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(AddConsoleCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.AddConsole(
            name: command.Name,
            manufacturer: command.Manufacturer,
            model: command.Model,
            region: command.Region,
            condition: command.Condition,
            completeness: command.Completeness,
            status: command.Status,
            releaseDate: command.ReleaseDate,
            purchaseDate: command.PurchaseDate,
            purchasePrice: command.PurchasePrice,
            purchaseOrigin: command.PurchaseOrigin,
            notes: command.Notes,
            thumbnailUrl: command.ThumbnailUrl);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
