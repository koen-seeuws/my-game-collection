using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.Repositories;

/// <summary>
/// Base repository that persists an aggregate through a separate EF data model (<typeparamref name="TEntity"/>).
/// Derived repositories supply the key lookup and the mapping in both directions.
/// </summary>
public abstract class BaseRepository<TAggregate, TAggregateKey, TEntity> : IRepository<TAggregate, TAggregateKey> 
    where TAggregate : IAggregate<TAggregateKey>
    where TEntity : class
{
    protected DbSet<TEntity> DbSet { get; }
    protected GameCollectionDbContext Context { get; }

    protected BaseRepository(GameCollectionDbContext dbContext)
    {
        DbSet = dbContext.Set<TEntity>();
        Context = dbContext;
    }

    public async Task<TAggregate?> GetAsync(TAggregateKey key, CancellationToken ct = default)
    {
        var entity = await FindEntityAsync(key, ct);

        return entity is null ? CreateNewAggregate(key) : ToAggregate(entity);
    }

    public async Task SaveAsync(TAggregate aggregate, CancellationToken ct = default)
    {
        var entity = await FindEntityAsync(aggregate.AggregateKey, ct);

        if (entity is null)
        {
            entity = CreateEntity(aggregate);
            DbSet.Add(entity);
        }

        ApplyToEntity(aggregate, entity);

        await Context.SaveChangesAsync(ct);
    }

    /// <summary>The query used to load the full aggregate (override to add <c>Include</c>s).</summary>
    protected virtual IQueryable<TEntity> Query() => DbSet;

    /// <summary>Predicate matching the entity that stores the aggregate with the given key.</summary>
    protected abstract Expression<Func<TEntity, bool>> HasKey(TAggregateKey key);

    /// <summary>The aggregate returned when nothing has been stored for the key yet.</summary>
    protected abstract TAggregate CreateNewAggregate(TAggregateKey key);

    /// <summary>Creates a new (empty) root entity for an aggregate that is saved for the first time.</summary>
    protected abstract TEntity CreateEntity(TAggregate aggregate);

    /// <summary>Maps a loaded entity graph to the domain aggregate.</summary>
    protected abstract TAggregate ToAggregate(TEntity entity);

    /// <summary>Synchronises the (tracked) entity graph with the state of the aggregate.</summary>
    protected abstract void ApplyToEntity(TAggregate aggregate, TEntity entity);

    private Task<TEntity?> FindEntityAsync(TAggregateKey key, CancellationToken ct) =>
        Query().SingleOrDefaultAsync(HasKey(key), ct);
}
