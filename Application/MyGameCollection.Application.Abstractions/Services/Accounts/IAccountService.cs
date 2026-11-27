namespace MyGameCollection.Application.Abstractions.Services.Accounts;

/// <summary>
/// Creates and inspects the accounts people sign in with. Runs inside the <see cref="Persistence.IAccountsUnitOfWork"/>
/// of the current use case.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates an account that signs in with <paramref name="email"/>. Validation problems, like a taken email or a weak
    /// password, come back as errors; anything else throws (rolling back the use case).
    /// </summary>
    Task<CreateAccountResult> CreateAccountAsync(string firstName, string lastName, string email, string password,
        bool isAdmin, CancellationToken cancellationToken = default);

    Task<bool> HasAdminAsync(CancellationToken cancellationToken = default);
}

/// <summary>The new account's id, or why it could not be created.</summary>
public record CreateAccountResult(string? UserId, IReadOnlyList<string> Errors)
{
    public bool Succeeded => UserId is not null;
}
