namespace MyGameCollection.Application.Abstractions;

/// <summary>Optional video game database (e.g. RAWG) used to enrich a game with developer, genre, year and box art.</summary>
public interface IGameMetadataProvider
{
    string Name { get; }

    /// <summary>False when the provider is not configured (e.g. no API key).</summary>
    bool IsEnabled { get; }

    Task<GameMetadata?> FindAsync(string title, IReadOnlyCollection<string> platformNames, CancellationToken cancellationToken = default);
}

public sealed record GameMetadata(
    string Source,
    string Title,
    int? ReleaseYear,
    string? Developer,
    string? Publisher,
    string? Genre,
    string? CoverImageUrl);
