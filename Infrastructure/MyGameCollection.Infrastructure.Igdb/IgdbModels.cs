using System.Text.Json.Serialization;

namespace MyGameCollection.Infrastructure.Igdb;

// The subset of IGDB's response shapes this app asks for (snake_case JSON).

internal sealed record IgdbGame(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("first_release_date")] long? FirstReleaseDate,
    [property: JsonPropertyName("cover")] IgdbImage? Cover,
    [property: JsonPropertyName("platforms")] IReadOnlyList<IgdbPlatformRef>? Platforms);

internal sealed record IgdbPlatformRef(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("abbreviation")] string? Abbreviation);

internal sealed record IgdbImage(
    [property: JsonPropertyName("image_id")] string? ImageId);

internal sealed record IgdbPlatform(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("abbreviation")] string? Abbreviation,
    [property: JsonPropertyName("alternative_name")] string? AlternativeName,
    [property: JsonPropertyName("platform_logo")] IgdbImage? PlatformLogo,
    [property: JsonPropertyName("versions")] IReadOnlyList<IgdbPlatformVersion>? Versions);

internal sealed record IgdbPlatformVersion(
    [property: JsonPropertyName("companies")] IReadOnlyList<IgdbPlatformVersionCompany>? Companies,
    [property: JsonPropertyName("platform_version_release_dates")] IReadOnlyList<IgdbReleaseDate>? ReleaseDates);

internal sealed record IgdbPlatformVersionCompany(
    [property: JsonPropertyName("company")] IgdbCompany? Company,
    [property: JsonPropertyName("manufacturer")] bool Manufacturer);

internal sealed record IgdbCompany(
    [property: JsonPropertyName("name")] string? Name);

internal sealed record IgdbReleaseDate(
    [property: JsonPropertyName("y")] int? Year);
