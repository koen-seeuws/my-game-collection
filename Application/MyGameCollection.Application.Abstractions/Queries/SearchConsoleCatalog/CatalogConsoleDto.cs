namespace MyGameCollection.Application.Abstractions.Queries.SearchConsoleCatalog;

/// <summary>A console (platform) as known by the external catalog, used to prefill the console form.</summary>
public record CatalogConsoleDto
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }
    public string? AlternativeName { get; init; }
    public string? Manufacturer { get; init; }
    public int? ReleaseYear { get; init; }
    public string? LogoUrl { get; init; }
}
