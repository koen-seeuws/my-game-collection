using System.Text.Json.Serialization;

namespace MyGameCollection.Infrastructure.ScanDex;

/// <summary>A barcode as ScanDex knows it. <see cref="IgdbMetadata"/> is empty while it is not matched to a game.</summary>
internal sealed record ScanDexLookup(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("igdb_metadata")] ScanDexIgdbMetadata? IgdbMetadata);

/// <summary>The IGDB game a barcode is matched to, and the IGDB platform the copy is for.</summary>
internal sealed record ScanDexIgdbMetadata(
    [property: JsonPropertyName("id")] long? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("platform")] ScanDexPlatform? Platform,
    // Older responses give the platform id on its own, without a platform object.
    [property: JsonPropertyName("platform_id")] long? PlatformId);

internal sealed record ScanDexPlatform(
    [property: JsonPropertyName("id")] long? Id,
    [property: JsonPropertyName("name")] string? Name);
