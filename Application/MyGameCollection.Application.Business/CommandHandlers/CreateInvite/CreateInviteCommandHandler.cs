using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Persistence;
using MyGameCollection.Application.Business.Decorators;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.CommandHandlers.CreateInvite;

[UnitOfWork(typeof(IAccountsUnitOfWork))]
public class CreateInviteCommandHandler : ICommandHandler<CreateInviteCommand, CreatedInviteDto>
{
    private readonly IRepository<Invite, Guid> _invites;
    private readonly IUserContext _userContext;
    private readonly TimeProvider _time;

    public CreateInviteCommandHandler(IRepository<Invite, Guid> invites, IUserContext userContext, TimeProvider time)
    {
        _invites = invites;
        _userContext = userContext;
        _time = time;
    }

    public async Task<CreatedInviteDto> HandleAsync(CreateInviteCommand command, CancellationToken cancellationToken)
    {
        _userContext.EnsureAdmin();

        var (invite, token) = InviteOperations.Create(_userContext.UserId, command.Note, command.ValidFor,
            _time.GetUtcNow().UtcDateTime);
        await _invites.SaveAsync(invite, cancellationToken);

        return new CreatedInviteDto(token, invite.ExpiresAt);
    }
}
