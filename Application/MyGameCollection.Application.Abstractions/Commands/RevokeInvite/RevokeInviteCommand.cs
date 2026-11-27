namespace MyGameCollection.Application.Abstractions.Commands.RevokeInvite;

/// <summary>Makes an unused invite link stop working (administrators only).</summary>
public record RevokeInviteCommand : ICommand
{
    public required Guid Key { get; init; }
}
