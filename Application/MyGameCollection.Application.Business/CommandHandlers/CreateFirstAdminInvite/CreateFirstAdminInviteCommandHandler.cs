using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.CreateFirstAdminInvite;

/// <summary>Run by the server at startup, not by a user.</summary>
[UnitOfWork(typeof(IAccountsUnitOfWork))]
public class CreateFirstAdminInviteCommandHandler : ICommandHandler<CreateFirstAdminInviteCommand, string?>
{
    private readonly IRepository<Invite, Guid> _invites;
    private readonly IAccountService _accounts;
    private readonly TimeProvider _time;

    public CreateFirstAdminInviteCommandHandler(IRepository<Invite, Guid> invites, IAccountService accounts,
        TimeProvider time)
    {
        _invites = invites;
        _accounts = accounts;
        _time = time;
    }

    public async Task<string?> HandleAsync(CreateFirstAdminInviteCommand command, CancellationToken cancellationToken)
    {
        if (await _accounts.HasAdminAsync(cancellationToken))
            return null;

        var now = _time.GetUtcNow().UtcDateTime;
        string token;
        if (await _invites.GetAsync(Invite.FirstAdminKey, cancellationToken) is { } invite)
        {
            token = invite.ReissueFirstAdmin(command.ValidFor, now);
        }
        else
        {
            (invite, token) = InviteOperations.CreateFirstAdmin(command.ValidFor, now);
        }

        await _invites.SaveAsync(invite, cancellationToken);
        return token;
    }
}
