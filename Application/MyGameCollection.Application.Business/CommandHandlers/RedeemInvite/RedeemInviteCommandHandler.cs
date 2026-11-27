using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.RedeemInvite;

/// <summary>Runs for someone who is not signed in yet: the invite token is the authorization.</summary>
[NonTransactional]
public class RedeemInviteCommandHandler : ICommandHandler<RedeemInviteCommand, RedeemInviteResultDto>
{
    private readonly IAccountService _accounts;

    public RedeemInviteCommandHandler(IAccountService accounts)
    {
        _accounts = accounts;
    }

    public Task<RedeemInviteResultDto> HandleAsync(RedeemInviteCommand command, CancellationToken cancellationToken)
    {
        var firstName = command.FirstName.Trim();
        var lastName = command.LastName.Trim();
        if (firstName.Length == 0 || lastName.Length == 0)
            return Task.FromResult(RedeemInviteResultDto.Failure("Enter your first and last name."));
        if (firstName.Length > RedeemInviteCommand.MaxNameLength || lastName.Length > RedeemInviteCommand.MaxNameLength)
            return Task.FromResult(RedeemInviteResultDto.Failure(
                $"Keep your names under {RedeemInviteCommand.MaxNameLength} characters."));

        return _accounts.RedeemInviteAsync(command.Token, firstName, lastName, command.Email.Trim(), command.Password,
            cancellationToken);
    }
}
