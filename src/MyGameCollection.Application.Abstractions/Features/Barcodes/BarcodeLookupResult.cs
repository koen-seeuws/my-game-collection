using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Abstractions.Features.Barcodes;

public sealed record BarcodeLookupResult
{
    public required string Barcode { get; init; }
    public bool Found { get; init; }
    /// <summary>Human readable list of where the data came from, e.g. "UPCitemdb + RAWG".</summary>
    public string? Source { get; init; }
    public string? RawTitle { get; init; }

    public ItemType? Type { get; init; }
    public string? Title { get; init; }
    public Guid? PlatformId { get; init; }
    public Region? Region { get; init; }
    public string? Publisher { get; init; }
    public string? Developer { get; init; }
    public string? Genre { get; init; }
    public int? ReleaseYear { get; init; }
    public string? CoverImageUrl { get; init; }

    /// <summary>Items in your collection that already have this barcode.</summary>
    public IReadOnlyList<ItemListDto> ExistingItems { get; init; } = [];

    /// <summary>Copies the found values into the form. Only overwrites fields when a value was found.</summary>
    public void ApplyTo(ItemEditModel model)
    {
        model.Barcode = Barcode;
        if (Type is { } type) model.Type = type;
        if (!string.IsNullOrWhiteSpace(Title)) model.Title = Title;
        if (PlatformId is { } platformId) model.PlatformId = platformId;
        if (Region is { } region and not Domain.Enums.Region.Unknown) model.Region = region;
        model.Publisher = Publisher ?? model.Publisher;
        model.Developer = Developer ?? model.Developer;
        model.Genre = Genre ?? model.Genre;
        model.ReleaseYear = ReleaseYear ?? model.ReleaseYear;
        model.CoverImageUrl = CoverImageUrl ?? model.CoverImageUrl;
    }
}
