namespace MyGameCollection.Application.Abstractions.Queries.SearchConsoleCatalog;

/// <summary>Searches the external platform catalog (IGDB) by name.</summary>
public record SearchConsoleCatalogQuery : IQuery<IReadOnlyList<CatalogConsoleDto>>
{
    public required string Search { get; init; }
}
