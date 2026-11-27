namespace MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;

/// <summary>
/// While nobody is an administrator, replaces any earlier administrator invite with a new one and replies with its
/// token (otherwise <c>null</c>), so whoever runs the server can create the first account. Run at startup.
/// </summary>
public record CreateFirstAdminInviteCommand : ICommandInterfaces<string?>
{
    public required TimeSpan ValidFor { get; init; }
}
