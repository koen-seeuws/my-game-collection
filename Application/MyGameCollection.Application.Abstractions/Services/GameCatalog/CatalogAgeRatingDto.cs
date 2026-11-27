namespace MyGameCollection.Application.Abstractions.Services.GameCatalog;

/// <summary>An age rating a game got from one rating board, e.g. PEGI 7.</summary>
public record CatalogAgeRatingDto
{
    /// <summary>The rating board, e.g. "PEGI", "ESRB" or "CERO".</summary>
    public required string Organization { get; init; }

    /// <summary>The rating within that board, e.g. "7", "E10+" or "B".</summary>
    public required string Rating { get; init; }

    /// <summary>"PEGI 7", as printed on the box.</summary>
    public override string ToString() => $"{Organization} {Rating}";
}
