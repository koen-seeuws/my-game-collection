using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetMyAccessories;

namespace MyGameCollection.Infrastructure.EFC.QueryHandlers.GetMyAccessories;

public record GetMyAccessoriesQueryHandler : IQueryHandler<GetMyAccessoriesQuery, IEnumerable<AccessoryDto>>
{
    private readonly GameCollectionDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyAccessoriesQueryHandler(GameCollectionDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<AccessoryDto>> HandleAsync(GetMyAccessoriesQuery query, CancellationToken ct = default)
    {
        var owner = _userContext.UserId;

        return await _dbContext.Accessories
            .Where(accessory => accessory.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .OrderBy(accessory => accessory.Position)
            .Select(accessory => new AccessoryDto
            {
                Key = accessory.Key,
                Name = accessory.Name,
                Type = accessory.Type,
                Platform = accessory.Platform,
                Manufacturer = accessory.Manufacturer,
                Model = accessory.Model,
                Colour = accessory.Colour,
                Quantity = accessory.Quantity,
                Region = accessory.Region,
                Condition = accessory.Condition,
                Completeness = accessory.Completeness,
                Status = accessory.Status,
                PurchaseDate = accessory.PurchaseDate,
                PurchasePrice = accessory.PurchasePrice,
                PurchaseOrigin = accessory.PurchaseOrigin,
                Notes = accessory.Notes,
                ThumbnailUrl = accessory.ThumbnailUrl
            })
            .ToListAsync(ct);
    }
}
