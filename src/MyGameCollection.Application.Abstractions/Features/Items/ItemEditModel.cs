using System.ComponentModel.DataAnnotations;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Abstractions.Features.Items;

/// <summary>Mutable input model used by the add/edit form.</summary>
public sealed class ItemEditModel
{
    public ItemType Type { get; set; } = ItemType.Game;

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pick a platform.")]
    public Guid? PlatformId { get; set; }

    public Region Region { get; set; } = Region.Pal;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public ItemStatus Status { get; set; } = ItemStatus.Owned;

    public bool HasBox { get; set; }
    public bool HasManual { get; set; }
    public bool IsSealed { get; set; }

    [StringLength(64)]
    public string? Barcode { get; set; }

    [Range(0, 1_000_000)]
    public decimal? PurchasePrice { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(0, 1_000_000)]
    public decimal? EstimatedValue { get; set; }

    [StringLength(200)]
    public string? StorageLocation { get; set; }

    [StringLength(4000)]
    public string? Notes { get; set; }

    [StringLength(2000), Url]
    public string? CoverImageUrl { get; set; }

    // Game specific
    [StringLength(200)] public string? Publisher { get; set; }
    [StringLength(200)] public string? Developer { get; set; }
    [StringLength(200)] public string? Genre { get; set; }
    [Range(1950, 2100)] public int? ReleaseYear { get; set; }

    // Console specific
    [StringLength(100)] public string? Model { get; set; }
    [StringLength(100)] public string? SerialNumber { get; set; }
    [StringLength(100)] public string? Edition { get; set; }
}
