namespace MyGameCollection.Application.Abstractions.Persistence;

/// <summary>
/// A transaction over the account store (accounts and invites), which is separate from the game collections'
/// <see cref="IUnitOfWork"/>.
/// </summary>
public interface IAccountsUnitOfWork : IUnitOfWork;
