using MyGameCollection.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetMyConsoles;

namespace MyGameCollection.Infrastructure.EFC.QueryHandlers.GetMyConsoles;

public record GetMyConsolesQueryHandler : IQueryHandler<GetMyConsolesQuery, IEnumerable<ConsoleDto>>
{
    private readonly GameCollectionDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyConsolesQueryHandler(GameCollectionDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<ConsoleDto>> HandleAsync(GetMyConsolesQuery query, CancellationToken ct = default)
    {
        var owner = _userContext.UserId;

        return await _dbContext.Consoles
            .Where(console => console.GameCollection.Owner.ToUpper() == owner.ToUpper())
            .OrderBy(console => console.Manufacturer).ThenBy(console => console.Name)
            .Select(console => new ConsoleDto
            {
                Key = console.Key,
                Name = console.Name,
                Manufacturer = console.Manufacturer,
                Model = console.Model,
                Colour = console.Colour,
                Region = console.Region,
                Condition = console.Condition,
                Completeness = console.Completeness,
                Status = console.Status,
                ReleaseDate = console.ReleaseYear == null ? null : new ReleaseDate(console.ReleaseYear.Value, console.ReleaseMonth, console.ReleaseDay),
                PurchaseDate = console.PurchaseDate,
                PurchasePrice = console.PurchasePrice,
                PurchaseOrigin = console.PurchaseOrigin,
                Notes = console.Notes,
                ThumbnailUrl = console.ThumbnailUrl,
                CatalogId = console.CatalogId
            })
            .ToListAsync(ct);
    }
}
