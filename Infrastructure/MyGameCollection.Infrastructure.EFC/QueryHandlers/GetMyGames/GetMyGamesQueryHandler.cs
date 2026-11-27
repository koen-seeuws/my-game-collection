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

        var games = await _dbContext.Games
            .Where(game => game.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .OrderBy(game => game.Platform).ThenBy(game => game.Title)
            .Select(game => new GameDto
            {
                Key = game.Key,
                Title = game.Title,
                Platform = game.Platform,
                Edition = game.Edition,
                Genres = game.Genres,
                Format = game.Format,
                Region = game.Region,
                Condition = game.Condition,
                Completeness = game.Completeness,
                Status = game.Status,
                ReleaseDate = game.ReleaseYear == null ? null : new ReleaseDate(game.ReleaseYear.Value, game.ReleaseMonth, game.ReleaseDay),
                PurchaseDate = game.PurchaseDate,
                PurchasePrice = game.PurchasePrice,
                PurchaseOrigin = game.PurchaseOrigin,
                Notes = game.Notes,
                ThumbnailUrl = game.ThumbnailUrl,
                Players = new PlayerModes
                {
                    SinglePlayer = game.SinglePlayer,
                    Multiplayer = game.Multiplayer,
                    MaxLocalPlayers = game.MaxLocalPlayers,
                    MaxOnlinePlayers = game.MaxOnlinePlayers,
                    CouchCoop = game.CouchCoop,
                    OnlineCoop = game.OnlineCoop,
                    SplitScreen = game.SplitScreen
                }
            })
            .ToListAsync(ct);

        // Rows without any player details mean "unknown", like a game the catalog knows nothing about.
        return games.Select(g => g.Players!.IsEmpty ? g with { Players = null } : g).ToList();
    }
}
