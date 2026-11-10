using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Infrastructure.Persistence;

namespace MyGameCollection.Application.Tests;

/// <summary>
/// Real application wiring (MediatR pipeline, handlers, repositories, mappers) on top of a configurable
/// EF provider. Photo storage and external APIs are replaced with in-memory fakes.
/// </summary>
internal sealed class TestHost : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public TestHost(Action<DbContextOptionsBuilder> configureDb)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddApplication();
        services.AddPersistence(configureDb);
        services.AddSingleton<FakePhotoStorage>();
        services.AddSingleton<IPhotoStorage>(sp => sp.GetRequiredService<FakePhotoStorage>());
        services.AddSingleton<IBarcodeLookupProvider>(Barcodes);
        _provider = services.BuildServiceProvider();
    }

    public static TestHost InMemory() => new(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

    public FakeBarcodeProvider Barcodes { get; } = new();

    public ISender Sender => _provider.GetRequiredService<ISender>();
    public FakePhotoStorage Storage => _provider.GetRequiredService<FakePhotoStorage>();

    public async Task<AppDbContext> CreateDbContextAsync() =>
        await _provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}

internal sealed class FakePhotoStorage : IPhotoStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public async Task<string> SaveAsync(Guid itemId, Stream content, string extension, CancellationToken ct = default)
    {
        var name = $"{itemId:N}/{Guid.NewGuid():N}{extension}";
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        Files[name] = buffer.ToArray();
        return name;
    }

    public Stream? OpenRead(string fileName) => Files.TryGetValue(fileName, out var bytes) ? new MemoryStream(bytes) : null;

    public Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        Files.Remove(fileName);
        return Task.CompletedTask;
    }

    public Task DeleteAllForItemAsync(Guid itemId, CancellationToken ct = default)
    {
        foreach (var key in Files.Keys.Where(k => k.StartsWith($"{itemId:N}/")).ToList()) Files.Remove(key);
        return Task.CompletedTask;
    }
}

internal sealed class FakeBarcodeProvider : IBarcodeLookupProvider
{
    public Dictionary<string, BarcodeProduct> Products { get; } = [];
    public string Name => "Fake";

    public Task<BarcodeProduct?> LookupAsync(string barcode, CancellationToken ct = default) =>
        Task.FromResult(Products.GetValueOrDefault(barcode));
}
