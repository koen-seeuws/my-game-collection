using System.Text.Json.Serialization;

namespace MyGameCollection.Infrastructure.Igdb;

// The subset of IGDB's response shapes this app asks for (snake_case JSON).

internal sealed record IgdbGame(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("version_title")] string? VersionTitle,
    [property: JsonPropertyName("first_release_date")] long? FirstReleaseDate,
    [property: JsonPropertyName("release_dates")] IReadOnlyList<IgdbReleaseDate>? ReleaseDates,
    [property: JsonPropertyName("cover")] IgdbImage? Cover,
    [property: JsonPropertyName("platforms")] IReadOnlyList<IgdbPlatformRef>? Platforms,
    [property: JsonPropertyName("genres")] IReadOnlyList<IgdbGenre>? Genres,
    [property: JsonPropertyName("game_modes")] IReadOnlyList<long>? GameModes = null,
    [property: JsonPropertyName("multiplayer_modes")] IReadOnlyList<IgdbMultiplayerMode>? MultiplayerModes = null,
    [property: JsonPropertyName("involved_companies")] IReadOnlyList<IgdbInvolvedCompany>? InvolvedCompanies = null);

/// <summary>A company's part in a game: it can have developed it, published it, or both (or only ported it).</summary>
internal sealed record IgdbInvolvedCompany(
    [property: JsonPropertyName("company")] IgdbCompany? Company,
    [property: JsonPropertyName("developer")] bool Developer = false,
    [property: JsonPropertyName("publisher")] bool Publisher = false);

internal sealed record IgdbGenre(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name);

/// <summary>How a game can be played with others on one platform (or on all, without <see cref="Platform"/>).</summary>
internal sealed record IgdbMultiplayerMode(
    [property: JsonPropertyName("platform")] long? Platform = null,
    [property: JsonPropertyName("offlinemax")] int? OfflineMax = null,
    [property: JsonPropertyName("offlinecoopmax")] int? OfflineCoopMax = null,
    [property: JsonPropertyName("offlinecoop")] bool? OfflineCoop = null,
    [property: JsonPropertyName("onlinemax")] int? OnlineMax = null,
    [property: JsonPropertyName("onlinecoopmax")] int? OnlineCoopMax = null,
    [property: JsonPropertyName("onlinecoop")] bool? OnlineCoop = null,
    [property: JsonPropertyName("splitscreen")] bool? SplitScreen = null);

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
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("platform_logo")] IgdbImage? PlatformLogo,
    [property: JsonPropertyName("companies")] IReadOnlyList<IgdbPlatformVersionCompany>? Companies,
    [property: JsonPropertyName("platform_version_release_dates")] IReadOnlyList<IgdbReleaseDate>? ReleaseDates);

internal sealed record IgdbPlatformVersionCompany(
    [property: JsonPropertyName("company")] IgdbCompany? Company,
    [property: JsonPropertyName("manufacturer")] bool Manufacturer);

internal sealed record IgdbCompany(
    [property: JsonPropertyName("name")] string? Name);

/// <summary>
/// A release of a game or platform version (one per platform / region). <see cref="Date"/> alone is misleading:
/// a release only known by its year is dated December 31st, so <see cref="DateFormat"/> says how precise it is.
/// </summary>
internal sealed record IgdbReleaseDate(
    [property: JsonPropertyName("date")] long? Date,
    [property: JsonPropertyName("date_format")] int? DateFormat,
    [property: JsonPropertyName("category")] int? Category,
    [property: JsonPropertyName("y")] int? Year,
    [property: JsonPropertyName("m")] int? Month);
