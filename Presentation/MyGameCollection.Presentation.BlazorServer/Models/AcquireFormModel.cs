using System.ComponentModel.DataAnnotations;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for moving a wishlist item into the collection.</summary>
public sealed class AcquireFormModel
{
    [Required]
    public DateTime? PurchaseDate { get; set; } = DateTime.Today;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }

    /// <summary>Bought new or secondhand; empty when unknown.</summary>
    public PurchaseOrigin? PurchaseOrigin { get; set; }
}
