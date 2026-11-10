using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Domain.Entities;
using MyGameCollection.Domain.Enums;
using MyGameCollection.Infrastructure.Persistence.Mapping;

namespace MyGameCollection.Infrastructure.Persistence.Repositories;

internal sealed class PlatformRepository(IDbContextFactory<AppDbContext> contextFactory) : IPlatformRepository
{
    public async Task<Platform?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Platforms.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return record is null ? null : PlatformMapper.ToDomain(record);
    }

    public async Task<IReadOnlyList<Platform>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var records = await db.Platforms.AsNoTracking().ToListAsync(ct);
        return records.Select(PlatformMapper.ToDomain).ToList();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        return await db.Platforms.AnyAsync(p => p.Id == id, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId = null, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var normalized = name.Trim().ToLower();
        return await db.Platforms.AnyAsync(p => p.Name.ToLower() == normalized && p.Id != exceptId, ct);
    }

    public async Task<bool> HasItemsAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        return await db.Items.AnyAsync(i => i.PlatformId == id, ct);
    }

    public async Task AddAsync(Platform platform, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        db.Platforms.Add(PlatformMapper.ToRecord(platform));
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Platform platform, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Platforms.FirstOrDefaultAsync(p => p.Id == platform.Id, ct)
                     ?? throw new KeyNotFoundException($"Platform {platform.Id} does not exist.");
        PlatformMapper.Apply(platform, record);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        var record = await db.Platforms.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (record is null) return;
        db.Platforms.Remove(record);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlatformDto>> GetSummariesAsync(CancellationToken ct = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(ct);
        return await db.Platforms.AsNoTracking()
            .OrderBy(p => p.Manufacturer).ThenBy(p => p.ReleaseYear).ThenBy(p => p.Name)
            .Select(p => new PlatformDto(
                p.Id, p.Name, p.Manufacturer, p.ReleaseYear, p.Aliases,
                p.Items.Count(i => i.Type == ItemType.Game),
                p.Items.Count(i => i.Type == ItemType.Console)))
            .ToListAsync(ct);
    }
}
