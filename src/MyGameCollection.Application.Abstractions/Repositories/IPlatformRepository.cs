using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Abstractions.Repositories;

public interface IPlatformRepository
{
    Task<Platform?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Platform>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid? exceptId = null, CancellationToken ct = default);
    Task<bool> HasItemsAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Platform platform, CancellationToken ct = default);
    Task UpdateAsync(Platform platform, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // --- Read side ---

    /// <summary>All platforms with the number of games and consoles on each.</summary>
    Task<IReadOnlyList<PlatformDto>> GetSummariesAsync(CancellationToken ct = default);
}
