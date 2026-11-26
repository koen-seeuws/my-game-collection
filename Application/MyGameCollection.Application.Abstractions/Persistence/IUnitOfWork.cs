namespace MyGameCollection.Application.Abstractions.Persistence;

/// <summary>Starts a database transaction spanning everything done within the current DI scope.</summary>
public interface IUnitOfWork
{
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default);
}

/// <summary>An open transaction. Disposing it without calling <see cref="CommitAsync"/> rolls it back.</summary>
public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}
