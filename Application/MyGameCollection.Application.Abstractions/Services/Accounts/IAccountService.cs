using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;

namespace MyGameCollection.Application.Abstractions.Services.Accounts;

/// <summary>
/// Stores accounts and invites. Every method is atomic on its own: accounts live outside the game collection's unit
/// of work, so the command handlers that use this service are not wrapped in a transaction.
/// </summary>
public interface IAccountService
{
    Task<CreatedInviteDto> CreateInviteAsync(string createdBy, string? note, TimeSpan validFor, bool grantsAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes an invite that has not been used or revoked yet; returns whether it was revoked.</summary>
    Task<bool> RevokeInviteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Revokes every unused invite that would make its account an administrator.</summary>
    Task RevokeOpenAdminInvitesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the account and marks the invite as used, together or not at all (so an invite never creates two
    /// accounts). Validation problems, like a taken email or a weak password, come back as errors.
    /// </summary>
    Task<RedeemInviteResultDto> RedeemInviteAsync(string token, string userName, string email, string password,
        CancellationToken cancellationToken = default);

    Task<bool> HasAdminAsync(CancellationToken cancellationToken = default);
}
