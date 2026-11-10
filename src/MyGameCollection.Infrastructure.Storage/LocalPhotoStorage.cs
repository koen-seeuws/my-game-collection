using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Storage;

/// <summary>Stores photos on the local file system as {root}/{itemId}/{photoGuid}.{ext}.</summary>
internal sealed class LocalPhotoStorage : IPhotoStorage
{
    private readonly string _root;

    public LocalPhotoStorage(IOptions<PhotoStorageOptions> options, IHostEnvironment environment)
    {
        var configured = options.Value.RootPath;
        _root = Path.GetFullPath(Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured));
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Guid itemId, Stream content, string extension, CancellationToken cancellationToken = default)
    {
        var relative = $"{itemId:N}/{Guid.CreateVersion7():N}{extension}";
        var fullPath = Resolve(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, cancellationToken);
        return relative;
    }

    public Stream? OpenRead(string fileName)
    {
        var fullPath = Resolve(fileName);
        return File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null;
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Resolve(fileName);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task DeleteAllForItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var folder = Resolve($"{itemId:N}");
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        return Task.CompletedTask;
    }

    /// <summary>Maps a relative name to a full path and refuses anything that escapes the root folder.</summary>
    private string Resolve(string relative)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid photo path.");
        return fullPath;
    }
}
