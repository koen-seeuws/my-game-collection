namespace MyGameCollection.Infrastructure.Persistence.Models;

public class ItemPhotoRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ThumbnailFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long SizeBytes { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
