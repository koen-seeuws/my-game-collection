using MyGameCollection.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetMyGames;

namespace MyGameCollection.Infrastructure.EFC.QueryHandlers.GetMyGames;

public record GetMyGamesQueryHandler : IQueryHandler<GetMyGamesQuery, IEnumerable<GameDto>>
{
    private readonly GameCollectionDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyGamesQueryHandler(GameCollectionDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<GameDto>> HandleAsync(GetMyGamesQuery query, CancellationToken ct = default)
    {
        var owner = _userContext.UserId;

        return await _dbContext.Games
            .Where(game => game.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .OrderBy(game => game.Platform).ThenBy(game => game.Title)
            .Select(game => new GameDto
            {
                Key = game.Key,
                Title = game.Title,
                Platform = game.Platform,
                Edition = game.Edition,
                Format = game.Format,
                Region = game.Region,
                Condition = game.Condition,
                Completeness = game.Completeness,
                Status = game.Status,
                ReleaseDate = game.ReleaseYear == null ? null : new ReleaseDate(game.ReleaseYear.Value, game.ReleaseMonth, game.ReleaseDay),
                PurchaseDate = game.PurchaseDate,
                PurchasePrice = game.PurchasePrice,
                Notes = game.Notes,
                ThumbnailUrl = game.ThumbnailUrl
            })
            .ToListAsync(ct);
    }
}
