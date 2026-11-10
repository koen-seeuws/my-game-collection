using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Abstractions.Repositories;

/// <summary>
/// Persistence port for the <see cref="CollectionItem"/> aggregate (including its photos).
/// Every write is a self-contained transaction.
/// </summary>
public interface IItemRepository
{
    /// <summary>Loads the item including its photos, or null when it does not exist.</summary>
    Task<CollectionItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(CollectionItem item, CancellationToken ct = default);

    /// <summary>Saves the item and synchronises its photo collection (added, removed and changed photos).</summary>
    Task UpdateAsync(CollectionItem item, CancellationToken ct = default);

    /// <summary>Deletes the item and its photo metadata. Returns false when it did not exist.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<ItemPhoto?> GetPhotoAsync(Guid photoId, CancellationToken ct = default);

    // --- Read side ---

    Task<PagedResult<ItemListDto>> SearchAsync(ItemFilter filter, CancellationToken ct = default);

    Task<CollectionStatistics> GetStatisticsAsync(CancellationToken ct = default);
}
