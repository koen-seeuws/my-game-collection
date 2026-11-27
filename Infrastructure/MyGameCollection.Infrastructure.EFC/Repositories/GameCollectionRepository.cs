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
        DbSet.Include(c => c.Games).ThenInclude(g => g.AddOns).Include(c => c.Consoles).Include(c => c.Accessories).AsSplitQuery();

    protected override Expression<Func<DataModel.GameCollection, bool>> HasKey(string key) =>
        c => c.Owner.ToUpper() == key.ToUpper();

    protected override Domain.GameCollection CreateNewAggregate(string key) => new()
    {
        AggregateKey = key.ToUpper(),
        Games = [],
        Consoles = [],
        Accessories = []
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
                Genres = g.Genres.ToList(),
                Developers = g.Developers.ToList(),
                Publishers = g.Publishers.ToList(),
                Series = g.Series,
                Type = g.Type,
                AgeRating = g.AgeRating,
                Format = g.Format,
                Players = ToPlayerModes(g),
                Region = g.Region,
                Condition = g.Condition,
                Completeness = g.Completeness,
                Status = g.Status,
                ReleaseDate = ToReleaseDate(g),
                PurchaseDate = g.PurchaseDate,
                PurchasePrice = g.PurchasePrice,
                PurchaseOrigin = g.PurchaseOrigin,
                PlayStatus = g.PlayStatus,
                CompletedOn = g.CompletedOn,
                Notes = g.Notes,
                ThumbnailUrl = g.ThumbnailUrl,
                CatalogId = g.CatalogId,
                AddOns = g.AddOns
                    .OrderBy(a => a.Position)
                    .Select(a => new Domain.AddOn
                    {
                        Key = a.Key,
                        Title = a.Title,
                        Type = a.Type,
                        Format = a.Format,
                        Condition = a.Condition,
                        Completeness = a.Completeness,
                        Status = a.Status,
                        ReleaseDate = ToReleaseDate(a),
                        PurchaseDate = a.PurchaseDate,
                        PurchasePrice = a.PurchasePrice,
                        PurchaseOrigin = a.PurchaseOrigin,
                        Notes = a.Notes,
                        ThumbnailUrl = a.ThumbnailUrl,
                        CatalogId = a.CatalogId
                    })
                    .ToImmutableList()
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
                Colour = c.Colour,
                Region = c.Region,
                Condition = c.Condition,
                Completeness = c.Completeness,
                Status = c.Status,
                ReleaseDate = ToReleaseDate(c),
                PurchaseDate = c.PurchaseDate,
                PurchasePrice = c.PurchasePrice,
                PurchaseOrigin = c.PurchaseOrigin,
                Notes = c.Notes,
                ThumbnailUrl = c.ThumbnailUrl,
                CatalogId = c.CatalogId
            })
            .ToImmutableList(),
        Accessories = entity.Accessories
            .OrderBy(a => a.Position)
            .Select(a => new Domain.Accessory
            {
                Key = a.Key,
                Name = a.Name,
                Type = a.Type,
                Platform = a.Platform,
                Manufacturer = a.Manufacturer,
                Model = a.Model,
                Colour = a.Colour,
                Quantity = a.Quantity,
                Region = a.Region,
                Condition = a.Condition,
                Completeness = a.Completeness,
                Status = a.Status,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                PurchaseOrigin = a.PurchaseOrigin,
                Notes = a.Notes,
                ThumbnailUrl = a.ThumbnailUrl
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
                row.Genres = g.Genres.ToList();
                row.Developers = g.Developers.ToList();
                row.Publishers = g.Publishers.ToList();
                row.Series = g.Series;
                row.Type = g.Type;
                row.AgeRating = g.AgeRating;
                row.CatalogId = g.CatalogId;
                row.Format = g.Format;
                row.PlayStatus = g.PlayStatus;
                row.CompletedOn = g.CompletedOn;
                ApplyPlayerModes(row, g.Players);
                row.Region = g.Region;
                ApplyCommon(row, g.Condition, g.Completeness, g.Status, g.ReleaseDate, g.PurchaseDate,
                    g.PurchasePrice, g.PurchaseOrigin, g.Notes, g.ThumbnailUrl);

                Synchronise(row.AddOns, g.AddOns, a => a.Key,
                    create: a => new DataModel.AddOn { Key = a.Key, Title = a.Title },
                    apply: (a, addOnRow) =>
                    {
                        addOnRow.Title = a.Title;
                        addOnRow.Type = a.Type;
                        addOnRow.Format = a.Format;
                        addOnRow.CatalogId = a.CatalogId;
                        ApplyCommon(addOnRow, a.Condition, a.Completeness, a.Status, a.ReleaseDate, a.PurchaseDate,
                            a.PurchasePrice, a.PurchaseOrigin, a.Notes, a.ThumbnailUrl);
                    });
            });

        Synchronise(entity.Consoles, aggregate.Consoles, c => c.Key,
            create: c => new DataModel.GameConsole { Key = c.Key, Name = c.Name, Manufacturer = c.Manufacturer },
            apply: (c, row) =>
            {
                row.Name = c.Name;
                row.Manufacturer = c.Manufacturer;
                row.Model = c.Model;
                row.Colour = c.Colour;
                row.CatalogId = c.CatalogId;
                row.Region = c.Region;
                ApplyCommon(row, c.Condition, c.Completeness, c.Status, c.ReleaseDate, c.PurchaseDate,
                    c.PurchasePrice, c.PurchaseOrigin, c.Notes, c.ThumbnailUrl);
            });

        Synchronise(entity.Accessories, aggregate.Accessories, a => a.Key,
            create: a => new DataModel.Accessory { Key = a.Key, Name = a.Name },
            apply: (a, row) =>
            {
                row.Name = a.Name;
                row.Type = a.Type;
                row.Platform = a.Platform;
                row.Manufacturer = a.Manufacturer;
                row.Model = a.Model;
                row.Colour = a.Colour;
                row.Quantity = a.Quantity;
                row.Region = a.Region;
                // Accessories have no release date or catalog id: the catalog does not know them.
                ApplyCommon(row, a.Condition, a.Completeness, a.Status, releaseDate: null, a.PurchaseDate,
                    a.PurchasePrice, a.PurchaseOrigin, a.Notes, a.ThumbnailUrl);
            });
    }

    /// <summary>
    /// Makes the tracked child rows match the items of the aggregate: removes rows of deleted items,
    /// adds rows for new items, and updates every row (including its position).
    /// </summary>
    private static void Synchronise<TItem, TRow>(List<TRow> rows, IReadOnlyList<TItem> items, Func<TItem, Guid> keyOf,
        Func<TItem, TRow> create, Action<TItem, TRow> apply)
        where TRow : ItemRow
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

    private static void ApplyCommon(ItemRow row,
        ItemCondition condition, Completeness completeness,
        CollectionStatus status, ReleaseDate? releaseDate, DateOnly? purchaseDate,
        decimal? purchasePrice, PurchaseOrigin? purchaseOrigin, string? notes, string? thumbnailUrl)
    {
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

    private static void ApplyPlayerModes(DataModel.Game row, PlayerModes? players)
    {
        row.SinglePlayer = players?.SinglePlayer ?? false;
        row.Multiplayer = players?.Multiplayer ?? false;
        row.MaxLocalPlayers = players?.MaxLocalPlayers;
        row.MaxOnlinePlayers = players?.MaxOnlinePlayers;
        row.CouchCoop = players?.CouchCoop ?? false;
        row.OnlineCoop = players?.OnlineCoop ?? false;
        row.SplitScreen = players?.SplitScreen ?? false;
    }

    private static PlayerModes? ToPlayerModes(DataModel.Game row) => new PlayerModes
    {
        SinglePlayer = row.SinglePlayer,
        Multiplayer = row.Multiplayer,
        MaxLocalPlayers = row.MaxLocalPlayers,
        MaxOnlinePlayers = row.MaxOnlinePlayers,
        CouchCoop = row.CouchCoop,
        OnlineCoop = row.OnlineCoop,
        SplitScreen = row.SplitScreen
    }.Normalised();

    private static ReleaseDate? ToReleaseDate(ItemRow row) =>
        row.ReleaseYear is { } year ? new ReleaseDate(year, row.ReleaseMonth, row.ReleaseDay) : null;
}
