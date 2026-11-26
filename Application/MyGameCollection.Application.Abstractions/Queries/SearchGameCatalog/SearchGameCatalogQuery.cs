namespace MyGameCollection.Application.Abstractions.Queries.SearchGameCatalog;

/// <summary>Searches the external game catalog (IGDB) by title.</summary>
public record SearchGameCatalogQuery : IQuery<IReadOnlyList<CatalogGameDto>>
{
    public required string Search { get; init; }
}
