using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>A model or revision of a console as known by the external catalog, e.g. "PlayStation 5 Pro".</summary>
public record CatalogConsoleVersionDto
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public ReleaseDate? ReleaseDate { get; init; }

    /// <summary>A small picture of this model, for showing in the model picker.</summary>
    public string? LogoUrl { get; init; }

    /// <summary>A larger picture of this model, stored as the console's thumbnail.</summary>
    public string? ThumbnailUrl { get; init; }
}
