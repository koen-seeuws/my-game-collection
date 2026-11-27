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
                Developers = game.Developers,
                Publishers = game.Publishers,
                Series = game.Series,
                Type = game.Type,
                AgeRating = game.AgeRating,
                Format = game.Format,
                Region = game.Region,
                Condition = game.Condition,
                Completeness = game.Completeness,
                Status = game.Status,
                ReleaseDate = game.ReleaseYear == null ? null : new ReleaseDate(game.ReleaseYear.Value, game.ReleaseMonth, game.ReleaseDay),
                PurchaseDate = game.PurchaseDate,
                PurchasePrice = game.PurchasePrice,
                PurchaseOrigin = game.PurchaseOrigin,
                PlayStatus = game.PlayStatus,
                CompletedOn = game.CompletedOn,
                Notes = game.Notes,
                ThumbnailUrl = game.ThumbnailUrl,
                CatalogId = game.CatalogId,
                Players = new PlayerModes
                {
                    SinglePlayer = game.SinglePlayer,
                    Multiplayer = game.Multiplayer,
                    MaxLocalPlayers = game.MaxLocalPlayers,
                    MaxOnlinePlayers = game.MaxOnlinePlayers,
                    CouchCoop = game.CouchCoop,
                    OnlineCoop = game.OnlineCoop,
                    SplitScreen = game.SplitScreen
                },
                AddOns = game.AddOns
                    .OrderBy(addOn => addOn.Position)
                    .Select(addOn => new AddOnDto
                    {
                        Key = addOn.Key,
                        Title = addOn.Title,
                        Type = addOn.Type,
                        Format = addOn.Format,
                        Condition = addOn.Condition,
                        Completeness = addOn.Completeness,
                        Status = addOn.Status,
                        ReleaseDate = addOn.ReleaseYear == null ? null : new ReleaseDate(addOn.ReleaseYear.Value, addOn.ReleaseMonth, addOn.ReleaseDay),
                        PurchaseDate = addOn.PurchaseDate,
                        PurchasePrice = addOn.PurchasePrice,
                        PurchaseOrigin = addOn.PurchaseOrigin,
                        Notes = addOn.Notes,
                        ThumbnailUrl = addOn.ThumbnailUrl,
                        CatalogId = addOn.CatalogId
                    })
                    .ToList()
            })
            .AsSplitQuery()
            .ToListAsync(ct);

        // Rows without any player details mean "unknown", like a game the catalog knows nothing about.
        return games.Select(g => g.Players!.IsEmpty ? g with { Players = null } : g).ToList();
    }
}
