using Microsoft.EntityFrameworkCore.Storage;
using MyGameCollection.Application.Abstractions.Persistence;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>
/// Opens a transaction on the scoped <see cref="AccountsDbContext"/>; every save on that context (by the invite
/// repository and by ASP.NET Core Identity's stores) automatically takes part in it.
/// </summary>
internal sealed class AccountsUnitOfWork : IAccountsUnitOfWork
{
    private readonly AccountsDbContext _db;

    public AccountsUnitOfWork(AccountsDbContext db)
    {
        _db = db;
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        new Transaction(await _db.Database.BeginTransactionAsync(ct));

    private sealed class Transaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public Transaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);

        // Rolls back when the transaction was not committed.
        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
