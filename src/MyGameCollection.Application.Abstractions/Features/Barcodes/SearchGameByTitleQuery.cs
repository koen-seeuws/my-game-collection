using System.ComponentModel.DataAnnotations;

namespace MyGameCollection.Application.Abstractions.Features.Barcodes;

/// <summary>Looks up game details by title (for games without a barcode, or when the barcode was unknown).</summary>
public sealed record SearchGameByTitleQuery([property: Required] string Title, Guid? PlatformId) : IRequest<BarcodeLookupResult?>;
