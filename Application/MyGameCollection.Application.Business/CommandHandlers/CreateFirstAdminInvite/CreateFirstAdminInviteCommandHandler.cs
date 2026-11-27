using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.CreateFirstAdminInvite;

/// <summary>Run by the server at startup, not by a user.</summary>
[NonTransactional]
public class CreateFirstAdminInviteCommandHandler : ICommandHandler<CreateFirstAdminInviteCommand, string?>
{
    private readonly IAccountService _accounts;

    public CreateFirstAdminInviteCommandHandler(IAccountService accounts)
    {
        _accounts = accounts;
    }

    public async Task<string?> HandleAsync(CreateFirstAdminInviteCommand command, CancellationToken cancellationToken)
    {
        if (await _accounts.HasAdminAsync(cancellationToken))
            return null;

        // Only the newest link works: the token of an earlier one was only ever logged.
        await _accounts.RevokeOpenAdminInvitesAsync(cancellationToken);

        var invite = await _accounts.CreateInviteAsync("system", "First administrator", command.ValidFor,
            grantsAdmin: true, cancellationToken);
        return invite.Token;
    }
}
