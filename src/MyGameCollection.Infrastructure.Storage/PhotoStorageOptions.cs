namespace MyGameCollection.Infrastructure.Storage;

public sealed class PhotoStorageOptions
{
    public const string SectionName = "PhotoStorage";

    /// <summary>Folder where photos are written. In Docker this is a mounted volume (default /data/photos).</summary>
    public string RootPath { get; set; } = "data/photos";
}
