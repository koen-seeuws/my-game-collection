using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Metadata;

/// <summary>
/// RAWG video game database (free tier, API key required). Used to fill in release year, developer,
/// publisher, genre and artwork after a barcode lookup, or when searching by title.
/// </summary>
internal sealed partial class RawgMetadataProvider(HttpClient http, IOptions<RawgOptions> options) : IGameMetadataProvider
{
    private string? ApiKey => options.Value.ApiKey;

    public string Name => "RAWG";

    public bool IsEnabled => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<GameMetadata?> FindAsync(string title, IReadOnlyCollection<string> platformNames, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled) return null;

        var key = Uri.EscapeDataString(ApiKey!);
        var search = await http.GetFromJsonAsync<RawgSearch>(
            $"games?key={key}&search={Uri.EscapeDataString(title)}&page_size=10", cancellationToken);
        if (search?.Results is not { Count: > 0 } results) return null;

        var wanted = Normalize(title);
        var best = results
            .Select(r => new
            {
                Game = r,
                Score = Similarity(wanted, Normalize(r.Name ?? "")),
                PlatformMatch = platformNames.Count > 0 && (r.Platforms ?? []).Any(p =>
                    platformNames.Any(n => string.Equals(n, p.Platform?.Name, StringComparison.OrdinalIgnoreCase)))
            })
            .Where(x => x.Score >= 0.5)
            .OrderByDescending(x => x.PlatformMatch)
            .ThenByDescending(x => x.Score)
            .FirstOrDefault();

        if (best is null) return null;

        var details = await http.GetFromJsonAsync<RawgDetails>($"games/{best.Game.Id}?key={key}", cancellationToken);

        return new GameMetadata(
            Name,
            best.Game.Name ?? title,
            best.Game.Released is { Length: >= 4 } released && int.TryParse(released[..4], out var year) ? year : null,
            details?.Developers?.FirstOrDefault()?.Name,
            details?.Publishers?.FirstOrDefault()?.Name,
            best.Game.Genres is { Count: > 0 } genres ? string.Join(", ", genres.Select(g => g.Name).Take(3)) : null,
            best.Game.BackgroundImage);
    }

    private static string Normalize(string s) => NonAlphaNumeric().Replace(s.ToLowerInvariant(), " ").Trim();

    /// <summary>Token overlap (Jaccard) between two normalised titles.</summary>
    internal static double Similarity(string a, string b)
    {
        var ta = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var tb = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        if (ta.Count == 0 || tb.Count == 0) return 0;
        return (double)ta.Intersect(tb).Count() / ta.Union(tb).Count();
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumeric();

    private sealed record RawgSearch([property: JsonPropertyName("results")] List<RawgGame>? Results);

    private sealed record RawgGame(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("released")] string? Released,
        [property: JsonPropertyName("background_image")] string? BackgroundImage,
        [property: JsonPropertyName("genres")] List<RawgNamed>? Genres,
        [property: JsonPropertyName("platforms")] List<RawgPlatformWrapper>? Platforms);

    private sealed record RawgPlatformWrapper([property: JsonPropertyName("platform")] RawgNamed? Platform);

    private sealed record RawgNamed([property: JsonPropertyName("name")] string? Name);

    private sealed record RawgDetails(
        [property: JsonPropertyName("developers")] List<RawgNamed>? Developers,
        [property: JsonPropertyName("publishers")] List<RawgNamed>? Publishers);
}
