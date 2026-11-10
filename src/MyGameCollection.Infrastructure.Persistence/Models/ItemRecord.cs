using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Infrastructure.Persistence.Models;

/// <summary>
/// Database shape of a collection item: one flat table for games and consoles, with <see cref="Type"/>
/// telling them apart. Mapped to/from the domain <c>Game</c> / <c>GameConsole</c> by <c>ItemMapper</c>.
/// </summary>
public class ItemRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public ItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid PlatformId { get; set; }
    public PlatformRecord? Platform { get; set; }

    public Region Region { get; set; }
    public ItemCondition Condition { get; set; }
    public ItemStatus Status { get; set; }
    public bool HasBox { get; set; }
    public bool HasManual { get; set; }
    public bool IsSealed { get; set; }

    public string? Barcode { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }
    public string? CoverImageUrl { get; set; }

    // Game columns
    public string? Publisher { get; set; }
    public string? Developer { get; set; }
    public string? Genre { get; set; }
    public int? ReleaseYear { get; set; }

    // Console columns
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Edition { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<ItemPhotoRecord> Photos { get; set; } = [];
}
