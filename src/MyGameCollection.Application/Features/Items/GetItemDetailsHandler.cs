using MediatR;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;
using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Features.Items;

internal sealed class GetItemDetailsHandler(IItemRepository items, IPlatformRepository platforms)
    : IRequestHandler<GetItemDetailsQuery, ItemDetailsDto>
{
    public async Task<ItemDetailsDto> Handle(GetItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException("Item", request.Id);
        var platform = await platforms.GetByIdAsync(item.PlatformId, cancellationToken);

        var game = item as Game;
        var console = item as GameConsole;

        return new ItemDetailsDto(
            item.Id, item.Type, item.Title, item.PlatformId, platform?.Name ?? "?", platform?.Manufacturer ?? "",
            item.Region, item.Condition, item.Status, item.HasBox, item.HasManual, item.IsSealed,
            item.Barcode, item.PurchasePrice, item.PurchaseDate, item.EstimatedValue, item.StorageLocation,
            item.Notes, item.CoverImageUrl,
            game?.Publisher, game?.Developer, game?.Genre, game?.ReleaseYear,
            console?.Model, console?.SerialNumber, console?.Edition,
            item.CreatedAt, item.UpdatedAt,
            item.Photos
                .OrderByDescending(p => p.IsPrimary).ThenBy(p => p.SortOrder)
                .Select(p => new PhotoDto(p.Id, p.OriginalFileName, p.IsPrimary, p.SortOrder, p.SizeBytes, p.CreatedAt))
                .ToList());
    }
}
