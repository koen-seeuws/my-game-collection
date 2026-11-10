using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Domain.Entities;
using MyGameCollection.Domain.Enums;
using MyGameCollection.Infrastructure.Persistence.Mapping;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Repositories;

/// <summary>
/// Maps the CollectionItem aggregate to <see cref="ItemRecord"/>/<see cref="ItemPhotoRecord"/>.
/// Each call uses its own short-lived DbContext: Blazor Server keeps DI scopes alive for a whole circuit,
/// so a shared context would see concurrent use.
/// </summary>
internal sealed class ItemRepository(IDbContextFactory<AppDbContext> contextFactory) : IItemRepository
{
    public async Task<CollectionItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Items.AsNoTracking().Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == id, ct);
        return record is null ? null : ItemMapper.ToDomain(record);
    }

    public async Task AddAsync(CollectionItem item, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = new ItemRecord { Id = item.Id };
        ItemMapper.Apply(item, record);
        record.Photos.AddRange(item.Photos.Select(ItemMapper.ToRecord));
        db.Items.Add(record);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CollectionItem item, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Items.Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == item.Id, ct)
                     ?? throw new KeyNotFoundException($"Item {item.Id} does not exist.");

        ItemMapper.Apply(item, record);
        SyncPhotos(db, item, record);
        await db.SaveChangesAsync(ct);
    }

    private static void SyncPhotos(AppDbContext db, CollectionItem item, ItemRecord record)
    {
        var wanted = item.Photos.ToDictionary(p => p.Id);

        foreach (var removed in record.Photos.Where(r => !wanted.ContainsKey(r.Id)).ToList())
        {
            record.Photos.Remove(removed);
            db.Photos.Remove(removed);
        }

        foreach (var photo in item.Photos)
        {
            var existing = record.Photos.FirstOrDefault(r => r.Id == photo.Id);
            if (existing is null)
            {
                var added = ItemMapper.ToRecord(photo);
                record.Photos.Add(added);
                db.Photos.Add(added);
            }
            else
            {
                existing.SortOrder = photo.SortOrder;
                existing.IsPrimary = photo.IsPrimary;
            }
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Items.Include(i => i.Photos).FirstOrDefaultAsync(i => i.Id == id, ct);
        if (record is null) return false;
        db.Items.Remove(record); // photo rows cascade
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ItemPhoto?> GetPhotoAsync(Guid photoId, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Photos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == photoId, ct);
        return record is null ? null : ItemMapper.ToDomain(record);
    }

    public async Task<PagedResult<ItemListDto>> SearchAsync(ItemFilter filter, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var query = ApplyFilter(db.Items.AsNoTracking(), filter);

        var total = await query.CountAsync(ct);
        var items = await ApplySort(query, filter)
            .Skip(filter.Page * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => new ItemListDto(
                i.Id, i.Type, i.Title, i.PlatformId, i.Platform!.Name,
                i.Region, i.Condition, i.Status, i.HasBox, i.HasManual, i.IsSealed,
                i.PurchasePrice, i.EstimatedValue, i.CreatedAt,
                i.Photos.Where(p => p.IsPrimary).Select(p => (Guid?)p.Id).FirstOrDefault(),
                i.CoverImageUrl))
            .ToListAsync(ct);

        return new PagedResult<ItemListDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<CollectionStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var all = db.Items.AsNoTracking();
        var owned = all.Where(i => i.Status == ItemStatus.Owned || i.Status == ItemStatus.ForSale);

        var games = await owned.CountAsync(i => i.Type == ItemType.Game, ct);
        var consoles = await owned.CountAsync(i => i.Type == ItemType.Console, ct);
        var complete = await owned.CountAsync(i => i.IsSealed || (i.HasBox && i.HasManual), ct);
        var wishlist = await all.CountAsync(i => i.Status == ItemStatus.Wishlist, ct);
        var spent = await all.Where(i => i.Status != ItemStatus.Wishlist).SumAsync(i => i.PurchasePrice ?? 0, ct);
        var value = await owned.SumAsync(i => i.EstimatedValue ?? 0, ct);

        // Group on plain columns only (grouping on a navigation like Platform.Name doesn't translate everywhere).
        var counts = await owned
            .GroupBy(i => i.PlatformId)
            .Select(g => new
            {
                PlatformId = g.Key,
                Games = g.Count(i => i.Type == ItemType.Game),
                Consoles = g.Count(i => i.Type == ItemType.Console)
            })
            .OrderByDescending(x => x.Games + x.Consoles)
            .Take(10)
            .ToListAsync(ct);

        var ids = counts.Select(c => c.PlatformId).ToList();
        var names = await db.Platforms.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var platforms = counts
            .Select(c => new PlatformCount(c.PlatformId, names.GetValueOrDefault(c.PlatformId, "?"), c.Games, c.Consoles))
            .OrderByDescending(p => p.Games + p.Consoles)
            .ThenBy(p => p.Platform)
            .ToList();

        return new CollectionStatistics(games, consoles, wishlist, complete, spent, value, platforms);
    }

    internal static IQueryable<ItemRecord> ApplyFilter(IQueryable<ItemRecord> query, ItemFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(term) ||
                (i.Barcode != null && i.Barcode.ToLower().Contains(term)) ||
                (i.Notes != null && i.Notes.ToLower().Contains(term)) ||
                (i.StorageLocation != null && i.StorageLocation.ToLower().Contains(term)) ||
                i.Platform!.Name.ToLower().Contains(term));
        }

        if (filter.Barcode is { } barcode)
            query = query.Where(i => i.Barcode == barcode);

        if (filter.Type is { } type)
            query = query.Where(i => i.Type == type);

        if (filter.PlatformIds.Count > 0)
            query = query.Where(i => filter.PlatformIds.Contains(i.PlatformId));

        if (filter.Regions.Count > 0)
            query = query.Where(i => filter.Regions.Contains(i.Region));

        if (filter.Statuses.Count > 0)
            query = query.Where(i => filter.Statuses.Contains(i.Status));

        if (filter.MinimumCondition is { } minCondition)
        {
            // Lower enum value = better condition. Conditions are stored as text, so compare against the
            // explicit set of acceptable values instead of using <=.
            var acceptable = Enum.GetValues<ItemCondition>()
                .Where(c => c != ItemCondition.Unknown && c <= minCondition)
                .ToList();
            query = query.Where(i => acceptable.Contains(i.Condition));
        }

        if (filter.CompleteOnly)
            query = query.Where(i => i.IsSealed || (i.HasBox && i.HasManual));

        if (filter.HasPhotos is { } hasPhotos)
            query = hasPhotos ? query.Where(i => i.Photos.Any()) : query.Where(i => !i.Photos.Any());

        return query;
    }

    private static IQueryable<ItemRecord> ApplySort(IQueryable<ItemRecord> query, ItemFilter filter) =>
        (filter.SortBy, filter.Descending) switch
        {
            (ItemSortField.Platform, false) => query.OrderBy(i => i.Platform!.Name).ThenBy(i => i.Title),
            (ItemSortField.Platform, true) => query.OrderByDescending(i => i.Platform!.Name).ThenBy(i => i.Title),
            (ItemSortField.CreatedAt, false) => query.OrderBy(i => i.CreatedAt),
            (ItemSortField.CreatedAt, true) => query.OrderByDescending(i => i.CreatedAt),
            (ItemSortField.PurchaseDate, false) => query.OrderBy(i => i.PurchaseDate).ThenBy(i => i.Title),
            (ItemSortField.PurchaseDate, true) => query.OrderByDescending(i => i.PurchaseDate).ThenBy(i => i.Title),
            (ItemSortField.PurchasePrice, false) => query.OrderBy(i => i.PurchasePrice).ThenBy(i => i.Title),
            (ItemSortField.PurchasePrice, true) => query.OrderByDescending(i => i.PurchasePrice).ThenBy(i => i.Title),
            (ItemSortField.EstimatedValue, false) => query.OrderBy(i => i.EstimatedValue).ThenBy(i => i.Title),
            (ItemSortField.EstimatedValue, true) => query.OrderByDescending(i => i.EstimatedValue).ThenBy(i => i.Title),
            (_, true) => query.OrderByDescending(i => i.Title),
            _ => query.OrderBy(i => i.Title)
        };
}
