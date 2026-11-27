using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;
using MyGameCollection.Application.Business.Decorators;

namespace MyGameCollection.Application.Business.CommandHandlers.Accounts;

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

        if (command.ValidFor <= TimeSpan.Zero || command.ValidFor > CreateInviteCommand.MaxValidity)
            throw new ArgumentException($"An invite must be valid for some time, up to {CreateInviteCommand.MaxValidity.TotalDays} days.");

        var note = string.IsNullOrWhiteSpace(command.Note) ? null : command.Note.Trim();
        if (note?.Length > CreateInviteCommand.MaxNoteLength)
            throw new ArgumentException($"Keep the note under {CreateInviteCommand.MaxNoteLength} characters.");

        return _accounts.CreateInviteAsync(_userContext.UserName, note, command.ValidFor, grantsAdmin: false,
            cancellationToken);
    }
}
