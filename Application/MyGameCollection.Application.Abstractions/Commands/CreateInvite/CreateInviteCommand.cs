namespace MyGameCollection.Application.Abstractions.Commands.CreateInvite;

/// <summary>Creates a single-use invite link to create an account (administrators only).</summary>
public record CreateInviteCommand : ICommandInterfaces<CreatedInviteDto>
{
    public const int MaxNoteLength = 200;
    public static readonly TimeSpan MaxValidity = TimeSpan.FromDays(90);

    /// <summary>Who the invite is meant for, as a reminder.</summary>
    public string? Note { get; init; }

    public required TimeSpan ValidFor { get; init; }
}

/// <param name="Token">The secret for the invite link; it is not stored and cannot be retrieved later.</param>
public record CreatedInviteDto(string Token, DateTime ExpiresAt);
