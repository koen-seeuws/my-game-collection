using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.Accounts;

/// <summary>Runs for someone who is not signed in yet: the invite token is the authorization.</summary>
[NonTransactional]
public class RedeemInviteCommandHandler : ICommandHandler<RedeemInviteCommand, RedeemInviteResultDto>
{
    private readonly IAccountService _accounts;

    public RedeemInviteCommandHandler(IAccountService accounts)
    {
        _accounts = accounts;
    }

    public Task<RedeemInviteResultDto> HandleAsync(RedeemInviteCommand command, CancellationToken cancellationToken) =>
        _accounts.RedeemInviteAsync(command.Token, command.UserName.Trim(), command.Email.Trim(), command.Password,
            cancellationToken);
}
