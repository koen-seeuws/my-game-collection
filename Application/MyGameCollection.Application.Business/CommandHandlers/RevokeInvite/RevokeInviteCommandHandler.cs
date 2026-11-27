using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.RevokeInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.RevokeInvite;

[NonTransactional]
public class RevokeInviteCommandHandler : ICommandHandler<RevokeInviteCommand>
{
    private readonly IAccountService _accounts;
    private readonly IUserContext _userContext;

    public RevokeInviteCommandHandler(IAccountService accounts, IUserContext userContext)
    {
        _accounts = accounts;
        _userContext = userContext;
    }

    public async Task HandleAsync(RevokeInviteCommand command, CancellationToken cancellationToken)
    {
        _userContext.EnsureAdmin();

        if (!await _accounts.RevokeInviteAsync(command.Id, cancellationToken))
            throw new InvalidOperationException("That invite has already been used or revoked.");
    }
}
