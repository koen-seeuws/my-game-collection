using Microsoft.EntityFrameworkCore.Storage;
using MyGameCollection.Application.Abstractions.Persistence;

namespace MyGameCollection.Infrastructure.EFC;

/// <summary>
/// Opens a transaction on the scoped <see cref="GameCollectionDbContext"/>; every <c>SaveChangesAsync</c> on that
/// context (e.g. from repositories) automatically takes part in it.
/// </summary>
internal sealed class EntityFrameworkCoreUnitOfWork : IUnitOfWork
{
    private readonly GameCollectionDbContext _dbContext;

    public EntityFrameworkCoreUnitOfWork(GameCollectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => new EfTransaction(await _dbContext.Database.BeginTransactionAsync(ct));

    private sealed class EfTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);

        // Rolls back when the transaction was not committed.
        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
