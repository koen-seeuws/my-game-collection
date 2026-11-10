namespace MyGameCollection.Application.Abstractions;

/// <summary>Stores photo files somewhere (local disk / Docker volume in practice).</summary>
public interface IPhotoStorage
{
    /// <summary>Saves the stream and returns the storage-relative file name.</summary>
    Task<string> SaveAsync(Guid itemId, Stream content, string extension, CancellationToken cancellationToken = default);

    /// <summary>Opens a stored file for reading, or returns null when it does not exist.</summary>
    Stream? OpenRead(string fileName);

    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);

    Task DeleteAllForItemAsync(Guid itemId, CancellationToken cancellationToken = default);
}
