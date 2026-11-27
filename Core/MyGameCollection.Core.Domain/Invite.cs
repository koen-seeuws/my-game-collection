using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>
/// A single-use link an administrator hands out so someone can create an account; there is no open registration.
/// The link holds the invite's key and a secret (see <see cref="InviteToken"/>); only a hash of the secret is kept.
/// </summary>
public record Invite : IAggregate<Guid>
{
    public const int MaxNoteLength = 200;
    public static readonly TimeSpan MaxValidity = TimeSpan.FromDays(90);

    /// <summary>The key of the one invite that creates the first administrator; it is reissued until used.</summary>
    public static readonly Guid FirstAdminKey = new("00000000-0000-0000-0000-00000000ad01");

    public required Guid AggregateKey { get; init; }

    /// <summary>SHA-256 hash (hex) of the secret in the invite link.</summary>
    public required string SecretHash { get; set; }

    /// <summary>Who the invite is meant for, as a reminder for the administrator.</summary>
    public string? Note { get; init; }

    /// <summary>Whether the account created with this invite becomes an administrator.</summary>
    public bool GrantsAdmin { get; init; }

    /// <summary>The id of the administrator who created the invite ("system" for the first administrator's).</summary>
    public required string CreatedBy { get; init; }

    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? RedeemedAt { get; set; }

    /// <summary>The email address of the account that was created with the invite.</summary>
    public string? RedeemedBy { get; set; }
}
