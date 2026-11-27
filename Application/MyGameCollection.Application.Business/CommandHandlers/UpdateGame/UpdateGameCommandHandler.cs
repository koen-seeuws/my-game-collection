using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.UpdateGame;

public class UpdateGameCommandHandler : ICommandHandler<UpdateGameCommand>
{
    private readonly IRepository<GameCollection, string> _repository;
    private readonly IUserContext _userContext;

    public UpdateGameCommandHandler(IRepository<GameCollection, string> repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task HandleAsync(UpdateGameCommand command, CancellationToken cancellationToken)
    {
        var owner = _userContext.UserId;
        var collection = await _repository.GetAsync(owner, cancellationToken)
            ?? throw new InvalidOperationException($"No game collection could be loaded for owner '{owner}'.");

        collection.UpdateGame(
            command.Key,
            title: command.Title,
            platform: command.Platform,
            edition: command.Edition,
            genres: command.Genres,
            developers: command.Developers,
            publishers: command.Publishers,
            series: command.Series,
            type: command.Type,
            ageRating: command.AgeRating,
            format: command.Format,
            players: command.Players,
            region: command.Region,
            condition: command.Condition,
            completeness: command.Completeness,
            status: command.Status,
            releaseDate: command.ReleaseDate,
            purchaseDate: command.PurchaseDate,
            purchasePrice: command.PurchasePrice,
            purchaseOrigin: command.PurchaseOrigin,
            playStatus: command.PlayStatus,
            completedOn: command.CompletedOn,
            notes: command.Notes,
            thumbnailUrl: command.ThumbnailUrl,
            catalogId: command.CatalogId);

        await _repository.SaveAsync(collection, cancellationToken);
    }
}
