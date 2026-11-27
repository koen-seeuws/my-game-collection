namespace MyGameCollection.Infrastructure.Identity;

/// <summary>
/// A single-use link an administrator hands out so someone can create an account; there is no open registration.
/// Only a hash of the secret token is stored: the link itself is shown once, when the invite is created.
/// </summary>
public sealed class Invite
{
    public int Id { get; set; }

    /// <summary>SHA-256 hash (hex) of the secret token in the invite link.</summary>
    public required string TokenHash { get; set; }

    /// <summary>Who the invite is meant for, as a reminder for the administrator.</summary>
    public string? Note { get; set; }

    /// <summary>Whether the account created with this invite becomes an administrator.</summary>
    public bool GrantsAdmin { get; set; }

    public required string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? RedeemedAt { get; set; }

    /// <summary>The user name of the account that was created with this invite.</summary>
    public string? RedeemedBy { get; set; }

    public InviteStatus GetStatus(DateTime utcNow) =>
        RedeemedAt is not null ? InviteStatus.Used
        : RevokedAt is not null ? InviteStatus.Revoked
        : ExpiresAt <= utcNow ? InviteStatus.Expired
        : InviteStatus.Open;
}

public enum InviteStatus
{
    Open,
    Used,
    Expired,
    Revoked
}
