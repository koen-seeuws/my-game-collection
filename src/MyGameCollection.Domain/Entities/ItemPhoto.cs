using MyGameCollection.Domain.Common;

namespace MyGameCollection.Domain.Entities;

/// <summary>A user-uploaded photo of a specific item. The files themselves live on disk; only metadata is kept here.</summary>
public class ItemPhoto : BaseEntity
{
    internal ItemPhoto(Guid itemId, string fileName, string thumbnailFileName, string contentType, string? originalFileName, long sizeBytes)
        : this(Guid.CreateVersion7(), itemId, fileName, thumbnailFileName, contentType, originalFileName, sizeBytes) { }

    private ItemPhoto(Guid id, Guid itemId, string fileName, string thumbnailFileName, string contentType, string? originalFileName, long sizeBytes)
        : base(id)
    {
        ItemId = itemId;
        FileName = fileName;
        ThumbnailFileName = thumbnailFileName;
        ContentType = contentType;
        OriginalFileName = originalFileName;
        SizeBytes = sizeBytes;
    }

    /// <summary>Rebuilds an existing photo (used by persistence).</summary>
    public static ItemPhoto Restore(Guid id, Guid itemId, string fileName, string thumbnailFileName, string contentType,
        string? originalFileName, long sizeBytes, int sortOrder, bool isPrimary) =>
        new(id, itemId, fileName, thumbnailFileName, contentType, originalFileName, sizeBytes)
        {
            SortOrder = sortOrder,
            IsPrimary = isPrimary
        };

    public Guid ItemId { get; }
    public string FileName { get; }
    public string ThumbnailFileName { get; }
    public string ContentType { get; }
    public string? OriginalFileName { get; }
    public long SizeBytes { get; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
