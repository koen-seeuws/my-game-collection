using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Common;
using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Features.Items;

/// <summary>Maps the item form contract to and from the domain model.</summary>
internal static class ItemEditModelMapper
{
    public static ItemEditModel ToEditModel(this CollectionItem item)
    {
        var model = new ItemEditModel
        {
            Type = item.Type,
            Title = item.Title,
            PlatformId = item.PlatformId,
            Region = item.Region,
            Condition = item.Condition,
            Status = item.Status,
            HasBox = item.HasBox,
            HasManual = item.HasManual,
            IsSealed = item.IsSealed,
            Barcode = item.Barcode,
            PurchasePrice = item.PurchasePrice,
            PurchaseDate = item.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
            EstimatedValue = item.EstimatedValue,
            StorageLocation = item.StorageLocation,
            Notes = item.Notes,
            CoverImageUrl = item.CoverImageUrl
        };

        switch (item)
        {
            case Game g:
                model.Publisher = g.Publisher;
                model.Developer = g.Developer;
                model.Genre = g.Genre;
                model.ReleaseYear = g.ReleaseYear;
                break;
            case GameConsole c:
                model.Model = c.Model;
                model.SerialNumber = c.SerialNumber;
                model.Edition = c.Edition;
                break;
        }

        return model;
    }

    /// <summary>Copies the form values onto a domain item (trimming text, normalizing the barcode).</summary>
    public static void ApplyTo(this ItemEditModel model, CollectionItem item)
    {
        item.Title = model.Title;
        item.PlatformId = model.PlatformId ?? throw new InvalidOperationException("Platform is required.");
        item.Region = model.Region;
        item.Condition = model.Condition;
        item.Status = model.Status;
        item.HasBox = model.HasBox;
        item.HasManual = model.HasManual;
        item.IsSealed = model.IsSealed;
        item.Barcode = BarcodeNormalizer.Normalize(model.Barcode);
        item.PurchasePrice = model.PurchasePrice;
        item.PurchaseDate = model.PurchaseDate is { } d ? DateOnly.FromDateTime(d) : null;
        item.EstimatedValue = model.EstimatedValue;
        item.StorageLocation = Clean(model.StorageLocation);
        item.Notes = Clean(model.Notes);
        item.CoverImageUrl = Clean(model.CoverImageUrl);

        switch (item)
        {
            case Game g:
                g.Publisher = Clean(model.Publisher);
                g.Developer = Clean(model.Developer);
                g.Genre = Clean(model.Genre);
                g.ReleaseYear = model.ReleaseYear;
                break;
            case GameConsole c:
                c.Model = Clean(model.Model);
                c.SerialNumber = Clean(model.SerialNumber);
                c.Edition = Clean(model.Edition);
                break;
        }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
