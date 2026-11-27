namespace MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;

/// <summary>
/// While nobody is an administrator, (re)issues the first administrator's invite and replies with the token for its
/// link (otherwise <c>null</c>), so whoever runs the server can create the first account. Earlier links stop working.
/// Run at startup.
/// </summary>
public record CreateFirstAdminInviteCommand : ICommandInterfaces<string?>
{
    public required TimeSpan ValidFor { get; init; }
}
