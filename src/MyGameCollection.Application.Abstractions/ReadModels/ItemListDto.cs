using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Abstractions.ReadModels;

public sealed record ItemListDto(
    Guid Id,
    ItemType Type,
    string Title,
    Guid PlatformId,
    string PlatformName,
    Region Region,
    ItemCondition Condition,
    ItemStatus Status,
    bool HasBox,
    bool HasManual,
    bool IsSealed,
    decimal? PurchasePrice,
    decimal? EstimatedValue,
    DateTimeOffset CreatedAt,
    Guid? PrimaryPhotoId,
    string? CoverImageUrl)
{
    public bool IsComplete => IsSealed || (HasBox && HasManual);
}
