using MyGameCollection.Domain.Entities;
using MyGameCollection.Domain.Enums;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Mapping;

internal static class ItemMapper
{
    public static CollectionItem ToDomain(ItemRecord r)
    {
        CollectionItem item = r.Type switch
        {
            ItemType.Console => new GameConsole(r.Id) { Model = r.Model, SerialNumber = r.SerialNumber, Edition = r.Edition },
            _ => new Game(r.Id) { Publisher = r.Publisher, Developer = r.Developer, Genre = r.Genre, ReleaseYear = r.ReleaseYear }
        };

        item.Title = r.Title;
        item.PlatformId = r.PlatformId;
        item.Region = r.Region;
        item.Condition = r.Condition;
        item.Status = r.Status;
        item.HasBox = r.HasBox;
        item.HasManual = r.HasManual;
        item.IsSealed = r.IsSealed;
        item.Barcode = r.Barcode;
        item.PurchasePrice = r.PurchasePrice;
        item.PurchaseDate = r.PurchaseDate;
        item.EstimatedValue = r.EstimatedValue;
        item.StorageLocation = r.StorageLocation;
        item.Notes = r.Notes;
        item.CoverImageUrl = r.CoverImageUrl;
        item.RestoreAudit(r.CreatedAt, r.UpdatedAt);
        item.RestorePhotos(r.Photos.Select(ToDomain));
        return item;
    }

    public static ItemPhoto ToDomain(ItemPhotoRecord r)
    {
        var photo = ItemPhoto.Restore(r.Id, r.ItemId, r.FileName, r.ThumbnailFileName, r.ContentType,
            r.OriginalFileName, r.SizeBytes, r.SortOrder, r.IsPrimary);
        photo.RestoreAudit(r.CreatedAt, r.UpdatedAt);
        return photo;
    }

    /// <summary>Copies the item's own fields onto the record (photos are synchronised separately).</summary>
    public static void Apply(CollectionItem item, ItemRecord r)
    {
        r.Type = item.Type;
        r.Title = item.Title;
        r.PlatformId = item.PlatformId;
        r.Region = item.Region;
        r.Condition = item.Condition;
        r.Status = item.Status;
        r.HasBox = item.HasBox;
        r.HasManual = item.HasManual;
        r.IsSealed = item.IsSealed;
        r.Barcode = item.Barcode;
        r.PurchasePrice = item.PurchasePrice;
        r.PurchaseDate = item.PurchaseDate;
        r.EstimatedValue = item.EstimatedValue;
        r.StorageLocation = item.StorageLocation;
        r.Notes = item.Notes;
        r.CoverImageUrl = item.CoverImageUrl;

        var game = item as Game;
        r.Publisher = game?.Publisher;
        r.Developer = game?.Developer;
        r.Genre = game?.Genre;
        r.ReleaseYear = game?.ReleaseYear;

        var console = item as GameConsole;
        r.Model = console?.Model;
        r.SerialNumber = console?.SerialNumber;
        r.Edition = console?.Edition;
    }

    public static ItemPhotoRecord ToRecord(ItemPhoto p) => new()
    {
        Id = p.Id,
        ItemId = p.ItemId,
        FileName = p.FileName,
        ThumbnailFileName = p.ThumbnailFileName,
        ContentType = p.ContentType,
        OriginalFileName = p.OriginalFileName,
        SizeBytes = p.SizeBytes,
        SortOrder = p.SortOrder,
        IsPrimary = p.IsPrimary
    };
}
