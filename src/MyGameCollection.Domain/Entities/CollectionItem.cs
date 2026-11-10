using MyGameCollection.Domain.Common;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Domain.Entities;

/// <summary>
/// Base type for everything in the collection (aggregate root; owns its photos).
/// The platform is referenced by id only.
/// </summary>
public abstract class CollectionItem : BaseEntity
{
    private readonly List<ItemPhoto> _photos = [];
    private string _title = string.Empty;

    protected CollectionItem() { }
    protected CollectionItem(Guid id) : base(id) { }

    public abstract ItemType Type { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title is required.", nameof(value));
            _title = value.Trim();
        }
    }

    public Guid PlatformId { get; set; }

    public Region Region { get; set; }
    public ItemCondition Condition { get; set; }
    public ItemStatus Status { get; set; }

    public bool HasBox { get; set; }
    public bool HasManual { get; set; }
    public bool IsSealed { get; set; }

    /// <summary>Complete in box: has both box and manual (or is still sealed).</summary>
    public bool IsComplete => IsSealed || (HasBox && HasManual);

    public string? Barcode { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }

    /// <summary>Optional remote image (e.g. box art found by a barcode / metadata lookup).</summary>
    public string? CoverImageUrl { get; set; }

    public IReadOnlyList<ItemPhoto> Photos => _photos;

    public ItemPhoto AddPhoto(string fileName, string thumbnailFileName, string contentType, string? originalFileName, long sizeBytes)
    {
        var photo = new ItemPhoto(Id, fileName, thumbnailFileName, contentType, originalFileName, sizeBytes)
        {
            SortOrder = _photos.Count == 0 ? 0 : _photos.Max(p => p.SortOrder) + 1,
            IsPrimary = _photos.All(p => !p.IsPrimary)
        };
        _photos.Add(photo);
        return photo;
    }

    public void SetPrimaryPhoto(Guid photoId)
    {
        if (_photos.All(p => p.Id != photoId))
            throw new InvalidOperationException("Photo does not belong to this item.");
        foreach (var p in _photos)
            p.IsPrimary = p.Id == photoId;
    }

    public ItemPhoto RemovePhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId)
                    ?? throw new InvalidOperationException("Photo does not belong to this item.");
        _photos.Remove(photo);
        if (photo.IsPrimary && _photos.Count > 0)
            _photos.OrderBy(p => p.SortOrder).First().IsPrimary = true;
        return photo;
    }

    /// <summary>Restores the photos when the item is rebuilt from storage.</summary>
    public void RestorePhotos(IEnumerable<ItemPhoto> photos)
    {
        _photos.Clear();
        _photos.AddRange(photos.OrderBy(p => p.SortOrder));
    }
}
