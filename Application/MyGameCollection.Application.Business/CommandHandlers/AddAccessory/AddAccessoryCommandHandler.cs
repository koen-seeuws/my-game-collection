using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AddAccessory;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.AddAccessory;

public class AddAccessoryCommandHandler : ICommandHandler<AddAccessoryCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public AddAccessoryCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(AddAccessoryCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.AddAccessory(
            name: command.Name,
            type: command.Type,
            platform: command.Platform,
            manufacturer: command.Manufacturer,
            model: command.Model,
            colour: command.Colour,
            quantity: command.Quantity,
            region: command.Region,
            condition: command.Condition,
            completeness: command.Completeness,
            status: command.Status,
            purchaseDate: command.PurchaseDate,
            purchasePrice: command.PurchasePrice,
            purchaseOrigin: command.PurchaseOrigin,
            notes: command.Notes,
            thumbnailUrl: command.ThumbnailUrl);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
