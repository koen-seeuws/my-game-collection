using System.ComponentModel.DataAnnotations;

namespace MyGameCollection.Application.Abstractions.Features.Platforms;

public sealed class PlatformEditModel
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Manufacturer { get; set; } = string.Empty;
    [Range(1950, 2100)] public int? ReleaseYear { get; set; }

    /// <summary>Comma separated alternative names, used to recognise the platform in barcode results.</summary>
    [StringLength(500)] public string? Aliases { get; set; }

    public IEnumerable<string> GetAliases() =>
        (Aliases ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
