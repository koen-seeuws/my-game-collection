using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>When a game came out on one platform in one part of the world.</summary>
public record CatalogReleaseDto
{
    /// <summary>The region it came out in; empty for a worldwide release.</summary>
    public Region? Region { get; init; }

    public required ReleaseDate ReleaseDate { get; init; }
}
