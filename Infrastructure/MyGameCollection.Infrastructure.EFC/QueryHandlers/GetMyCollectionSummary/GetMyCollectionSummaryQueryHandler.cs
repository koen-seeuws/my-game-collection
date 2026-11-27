using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetMyCollectionSummary;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.QueryHandlers.GetMyCollectionSummary;

public record GetMyCollectionSummaryQueryHandler : IQueryHandler<GetMyCollectionSummaryQuery, CollectionSummaryDto>
{
    private readonly GameCollectionDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyCollectionSummaryQueryHandler(GameCollectionDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<CollectionSummaryDto> HandleAsync(GetMyCollectionSummaryQuery query, CancellationToken ct = default)
    {
        var owner = _userContext.UserId;

        // Only the few columns needed are loaded; the figures are computed in memory because SQLite
        // cannot aggregate decimals, and a personal collection is small enough for that.
        var games = await _dbContext.Games
            .Where(g => g.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .Select(g => new { g.Platform, g.Status, g.Format, g.Completeness, g.PlayStatus, g.PurchasePrice })
            .ToListAsync(ct);

        var consoles = await _dbContext.Consoles
            .Where(c => c.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .Select(c => new { c.Name, c.Status, c.PurchasePrice })
            .ToListAsync(ct);

        var addOns = await _dbContext.AddOns
            .Where(a => a.Game.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .Select(a => new { a.Status, a.PurchasePrice })
            .ToListAsync(ct);

        var accessories = await _dbContext.Accessories
            .Where(a => a.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .Select(a => new { a.Status, a.Quantity, a.PurchasePrice })
            .ToListAsync(ct);

        var ownedGames = games.Where(g => g.Status == CollectionStatus.Owned).ToList();
        var ownedConsoles = consoles.Where(c => c.Status == CollectionStatus.Owned).ToList();
        var ownedAddOns = addOns.Where(a => a.Status == CollectionStatus.Owned).ToList();
        var ownedAccessories = accessories.Where(a => a.Status == CollectionStatus.Owned).ToList();
        var ownedConsoleNames = ownedConsoles.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var platforms = games
            .Select(g => g.Platform)
            .Concat(ownedConsoleNames)
            .GroupBy(platform => platform, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PlatformSummaryDto
            {
                Platform = group.First(),
                OwnedGames = ownedGames.Count(g => string.Equals(g.Platform, group.Key, StringComparison.OrdinalIgnoreCase)),
                WishlistGames = games.Count(g => g.Status == CollectionStatus.Wishlist
                                                 && string.Equals(g.Platform, group.Key, StringComparison.OrdinalIgnoreCase)),
                HasConsole = ownedConsoleNames.Contains(group.Key)
            })
            .OrderByDescending(p => p.OwnedGames)
            .ThenBy(p => p.Platform, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CollectionSummaryDto
        {
            OwnedGames = ownedGames.Count,
            WishlistGames = games.Count - ownedGames.Count,
            OwnedConsoles = ownedConsoles.Count,
            WishlistConsoles = consoles.Count - ownedConsoles.Count,
            OwnedAddOns = ownedAddOns.Count,
            WishlistAddOns = addOns.Count - ownedAddOns.Count,
            OwnedAccessories = ownedAccessories.Sum(a => a.Quantity),
            WishlistAccessories = accessories.Count - ownedAccessories.Count,
            TotalSpent = ownedGames.Sum(g => g.PurchasePrice ?? 0) + ownedConsoles.Sum(c => c.PurchasePrice ?? 0)
                         + ownedAddOns.Sum(a => a.PurchasePrice ?? 0) + ownedAccessories.Sum(a => a.PurchasePrice ?? 0),
            Platforms = platforms,
            OwnedGamesByFormat = Enum.GetValues<GameFormat>()
                .ToDictionary(f => f, f => ownedGames.Count(g => g.Format == f)),
            OwnedGamesByCompleteness = Enum.GetValues<Completeness>()
                .ToDictionary(c => c, c => ownedGames.Count(g => g.Format == GameFormat.Physical && g.Completeness == c)),
            OwnedGamesByPlayStatus = Enum.GetValues<PlayStatus>()
                .ToDictionary(s => s, s => ownedGames.Count(g => g.PlayStatus == s))
        };
    }
}
