using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record PhotoDto(Guid Id, string? OriginalFileName, bool IsPrimary, int SortOrder, long SizeBytes, DateTimeOffset UploadedAt);

public sealed record ItemDetailsDto(
    Guid Id,
    ItemType Type,
    string Title,
    Guid PlatformId,
    string PlatformName,
    string PlatformManufacturer,
    Region Region,
    ItemCondition Condition,
    ItemStatus Status,
    bool HasBox,
    bool HasManual,
    bool IsSealed,
    string? Barcode,
    decimal? PurchasePrice,
    DateOnly? PurchaseDate,
    decimal? EstimatedValue,
    string? StorageLocation,
    string? Notes,
    string? CoverImageUrl,
    // Game specific
    string? Publisher,
    string? Developer,
    string? Genre,
    int? ReleaseYear,
    // Console specific
    string? Model,
    string? SerialNumber,
    string? Edition,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<PhotoDto> Photos)
{
    public bool IsComplete => IsSealed || (HasBox && HasManual);
}
