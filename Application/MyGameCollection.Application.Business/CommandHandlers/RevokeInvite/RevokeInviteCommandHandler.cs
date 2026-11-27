using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.RevokeInvite;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.RevokeInvite;

[UnitOfWork(typeof(IAccountsUnitOfWork))]
public class RevokeInviteCommandHandler : ICommandHandler<RevokeInviteCommand>
{
    private readonly IRepository<Invite, Guid> _invites;
    private readonly IUserContext _userContext;
    private readonly TimeProvider _time;

    public RevokeInviteCommandHandler(IRepository<Invite, Guid> invites, IUserContext userContext, TimeProvider time)
    {
        _invites = invites;
        _userContext = userContext;
        _time = time;
    }

    public async Task HandleAsync(RevokeInviteCommand command, CancellationToken cancellationToken)
    {
        _userContext.EnsureAdmin();

        var invite = await _invites.GetAsync(command.Key, cancellationToken)
            ?? throw new KeyNotFoundException($"There is no invite '{command.Key}'.");

        invite.Revoke(_time.GetUtcNow().UtcDateTime);
        await _invites.SaveAsync(invite, cancellationToken);
    }
}
