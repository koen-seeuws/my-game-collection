using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Abstractions.Persistence;

public interface IRepository<TAggregate, TKey> where TAggregate : IAggregate<TKey>
{
    Task<TAggregate?> GetAsync(TKey key, CancellationToken ct = default);
    Task SaveAsync(TAggregate aggregate, CancellationToken ct = default);
}
