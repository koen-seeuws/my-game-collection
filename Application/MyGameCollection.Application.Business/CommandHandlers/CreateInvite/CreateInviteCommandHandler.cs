using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.CreateInvite;

[NonTransactional]
public class CreateInviteCommandHandler : ICommandHandler<CreateInviteCommand, CreatedInviteDto>
{
    private readonly IAccountService _accounts;
    private readonly IUserContext _userContext;

    public CreateInviteCommandHandler(IAccountService accounts, IUserContext userContext)
    {
        _accounts = accounts;
        _userContext = userContext;
    }

    public Task<CreatedInviteDto> HandleAsync(CreateInviteCommand command, CancellationToken cancellationToken)
    {
        _userContext.EnsureAdmin();

        var note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim();

        return _accounts.CreateInviteAsync(_userContext.UserId, note, command.ValidFor, grantsAdmin: false,
            cancellationToken);
    }
}
