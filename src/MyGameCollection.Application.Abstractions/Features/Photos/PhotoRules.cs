namespace MyGameCollection.Application.Abstractions.Features.Photos;

/// <summary>A photo to upload. The UI supplies both the (resized) full image and a thumbnail.</summary>
public sealed record PhotoUpload(Stream Full, Stream Thumbnail, string ContentType, string? OriginalFileName, long SizeBytes);

public sealed record PhotoFile(Stream Content, string ContentType, DateTimeOffset LastModified);

public static class PhotoRules
{
    public const long MaxPhotoBytes = 25 * 1024 * 1024;

    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public static bool IsSupportedContentType(string contentType) => Extensions.ContainsKey(contentType);

    /// <summary>File extension for a supported content type, or null.</summary>
    public static string? ExtensionFor(string contentType) => Extensions.GetValueOrDefault(contentType);
}
