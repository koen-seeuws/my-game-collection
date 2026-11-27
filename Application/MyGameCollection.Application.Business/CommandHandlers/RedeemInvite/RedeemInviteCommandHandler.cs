using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.RedeemInvite;

/// <summary>
/// Runs for someone who is not signed in yet: the invite token is the authorization. The account and the used invite
/// are saved in one <see cref="IAccountsUnitOfWork"/> transaction, so an invite never creates two accounts.
/// </summary>
[UnitOfWork(typeof(IAccountsUnitOfWork))]
public class RedeemInviteCommandHandler : ICommandHandler<RedeemInviteCommand, RedeemInviteResultDto>
{
    private const string InvalidLink = "This invite link is invalid, has expired or has already been used.";

    private readonly IRepository<Invite, Guid> _invites;
    private readonly IAccountService _accounts;
    private readonly TimeProvider _time;

    public RedeemInviteCommandHandler(IRepository<Invite, Guid> invites, IAccountService accounts, TimeProvider time)
    {
        _invites = invites;
        _accounts = accounts;
        _time = time;
    }

    public async Task<RedeemInviteResultDto> HandleAsync(RedeemInviteCommand command, CancellationToken cancellationToken)
    {
        var firstName = command.FirstName.Trim();
        var lastName = command.LastName.Trim();
        var email = command.Email.Trim();
        if (firstName.Length == 0 || lastName.Length == 0)
            return RedeemInviteResultDto.Failure("Enter your first and last name.");
        if (firstName.Length > RedeemInviteCommand.MaxNameLength || lastName.Length > RedeemInviteCommand.MaxNameLength)
            return RedeemInviteResultDto.Failure($"Keep your names under {RedeemInviteCommand.MaxNameLength} characters.");

        var now = _time.GetUtcNow().UtcDateTime;
        if (!InviteToken.TryParse(command.Token, out var key, out var secret)
            || await _invites.GetAsync(key, cancellationToken) is not { } invite
            || !invite.CanBeRedeemedWith(secret, now))
            return RedeemInviteResultDto.Failure(InvalidLink);

        var account = await _accounts.CreateAccountAsync(firstName, lastName, email, command.Password,
            invite.GrantsAdmin, cancellationToken);
        if (!account.Succeeded)
            return RedeemInviteResultDto.Failure(account.Errors.ToArray());

        invite.Redeem(secret, email, now);
        await _invites.SaveAsync(invite, cancellationToken);

        return RedeemInviteResultDto.Success(account.UserId!);
    }
}
