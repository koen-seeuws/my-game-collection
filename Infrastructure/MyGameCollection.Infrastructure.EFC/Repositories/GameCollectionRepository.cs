using System.Collections.Immutable;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.EFC.DataModel;
using Domain = MyGameCollection.Core.Domain;

namespace MyGameCollection.Infrastructure.EFC.Repositories;

public class GameCollectionRepository : BaseRepository<Domain.GameCollection, string, DataModel.GameCollection>
{
    public GameCollectionRepository(GameCollectionDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<DataModel.GameCollection> Query() =>
        DbSet.Include(c => c.Games).Include(c => c.Consoles).AsSplitQuery();

    protected override Expression<Func<DataModel.GameCollection, bool>> HasKey(string key) =>
        c => c.Owner.ToUpper() == key.ToUpper();

    protected override Domain.GameCollection CreateNewAggregate(string key) => new()
    {
        AggregateKey = key.ToUpper(),
        Games = [],
        Consoles = []
    };

    protected override DataModel.GameCollection CreateEntity(Domain.GameCollection aggregate) => new()
    {
        Owner = aggregate.AggregateKey
    };

    protected override Domain.GameCollection ToAggregate(DataModel.GameCollection entity) => new()
    {
        AggregateKey = entity.Owner,
        Games = entity.Games
            .OrderBy(g => g.Position)
            .Select(g => new Domain.Game
            {
                Key = g.Key,
                Title = g.Title,
                Platform = g.Platform,
                Edition = g.Edition,
                Format = g.Format,
                Region = g.Region,
                Condition = g.Condition,
                Completeness = g.Completeness,
                Status = g.Status,
                ReleaseDate = ToReleaseDate(g),
                PurchaseDate = g.PurchaseDate,
                PurchasePrice = g.PurchasePrice,
                PurchaseOrigin = g.PurchaseOrigin,
                Notes = g.Notes,
                ThumbnailUrl = g.ThumbnailUrl
            })
            .ToImmutableList(),
        Consoles = entity.Consoles
            .OrderBy(c => c.Position)
            .Select(c => new Domain.GameConsole
            {
                Key = c.Key,
                Name = c.Name,
                Manufacturer = c.Manufacturer,
                Model = c.Model,
                Region = c.Region,
                Condition = c.Condition,
                Completeness = c.Completeness,
                Status = c.Status,
                ReleaseDate = ToReleaseDate(c),
                PurchaseDate = c.PurchaseDate,
                PurchasePrice = c.PurchasePrice,
                PurchaseOrigin = c.PurchaseOrigin,
                Notes = c.Notes,
                ThumbnailUrl = c.ThumbnailUrl
            })
            .ToImmutableList()
    };

    protected override void ApplyToEntity(Domain.GameCollection aggregate, DataModel.GameCollection entity)
    {
        Synchronise(entity.Games, aggregate.Games, g => g.Key,
            create: g => new DataModel.Game { Key = g.Key, Title = g.Title, Platform = g.Platform },
            apply: (g, row) =>
            {
                row.Title = g.Title;
                row.Platform = g.Platform;
                row.Edition = g.Edition;
                row.Format = g.Format;
                ApplyCommon(row, g.Region, g.Condition, g.Completeness, g.Status, g.ReleaseDate, g.PurchaseDate,
                    g.PurchasePrice, g.PurchaseOrigin, g.Notes, g.ThumbnailUrl);
            });

        Synchronise(entity.Consoles, aggregate.Consoles, c => c.Key,
            create: c => new DataModel.GameConsole { Key = c.Key, Name = c.Name, Manufacturer = c.Manufacturer },
            apply: (c, row) =>
            {
                row.Name = c.Name;
                row.Manufacturer = c.Manufacturer;
                row.Model = c.Model;
                ApplyCommon(row, c.Region, c.Condition, c.Completeness, c.Status, c.ReleaseDate, c.PurchaseDate,
                    c.PurchasePrice, c.PurchaseOrigin, c.Notes, c.ThumbnailUrl);
            });
    }

    /// <summary>
    /// Makes the tracked child rows match the items of the aggregate: removes rows of deleted items,
    /// adds rows for new items, and updates every row (including its position).
    /// </summary>
    private static void Synchronise<TItem, TRow>(List<TRow> rows, IReadOnlyList<TItem> items, Func<TItem, Guid> keyOf,
        Func<TItem, TRow> create, Action<TItem, TRow> apply)
        where TRow : CollectionItem
    {
        var existing = rows.ToDictionary(r => r.Key);
        var keysInAggregate = items.Select(keyOf).ToHashSet();

        // Deleted: removing from the required relationship makes EF delete the orphaned row.
        rows.RemoveAll(r => !keysInAggregate.Contains(r.Key));

        for (var position = 0; position < items.Count; position++)
        {
            var item = items[position];

            if (!existing.TryGetValue(keyOf(item), out var row))
            {
                // Added
                row = create(item);
                rows.Add(row);
            }

            apply(item, row);
            row.Position = position;
        }
    }

    private static void ApplyCommon(CollectionItem row, Region region,
        ItemCondition condition, Completeness completeness,
        CollectionStatus status, ReleaseDate? releaseDate, DateOnly? purchaseDate,
        decimal? purchasePrice, PurchaseOrigin? purchaseOrigin, string? notes, string? thumbnailUrl)
    {
        row.Region = region;
        row.Condition = condition;
        row.Completeness = completeness;
        row.Status = status;
        row.ReleaseYear = releaseDate?.Year;
        row.ReleaseMonth = releaseDate?.Month;
        row.ReleaseDay = releaseDate?.Day;
        row.PurchaseDate = purchaseDate;
        row.PurchasePrice = purchasePrice;
        row.PurchaseOrigin = purchaseOrigin;
        row.Notes = notes;
        row.ThumbnailUrl = thumbnailUrl;
    }

    private static ReleaseDate? ToReleaseDate(CollectionItem row) =>
        row.ReleaseYear is { } year ? new ReleaseDate(year, row.ReleaseMonth, row.ReleaseDay) : null;
}
