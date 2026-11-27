using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>An add-on (DLC, expansion...) as known by the external catalog, used to prefill the add-on form.</summary>
public record CatalogAddOnDto
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required AddOnType Type { get; init; }

    /// <summary>The name of the game it extends, when the catalog knows it.</summary>
    public string? GameName { get; init; }

    public ReleaseDate? ReleaseDate { get; init; }

    /// <summary>A small cover, for showing in search results.</summary>
    public string? CoverUrl { get; init; }

    /// <summary>A larger cover, stored as the add-on's thumbnail.</summary>
    public string? ThumbnailUrl { get; init; }
}
