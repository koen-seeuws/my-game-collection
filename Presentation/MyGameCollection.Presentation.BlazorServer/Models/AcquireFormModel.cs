using System.ComponentModel.DataAnnotations;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Input for moving a wishlist item into the collection.</summary>
public sealed class AcquireFormModel
{
    [Required]
    public DateTime? PurchaseDate { get; set; } = DateTime.Today;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal? PurchasePrice { get; set; }
}
