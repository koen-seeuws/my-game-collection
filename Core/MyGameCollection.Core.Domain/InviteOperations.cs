using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

public static class InviteOperations
{
    /// <summary>Creates an invite for one new (regular) account, with the token for its link.</summary>
    public static (Invite Invite, string Token) Create(string createdBy, string? note, TimeSpan validFor, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);
        EnsureValidValidity(validFor);

        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (note?.Length > Invite.MaxNoteLength)
            throw new ArgumentException($"Keep the note under {Invite.MaxNoteLength} characters.", nameof(note));

        var key = Guid.NewGuid();
        var (secret, hash) = InviteToken.NewSecret();
        var invite = new Invite
        {
            AggregateKey = key,
            SecretHash = hash,
            Note = note,
            CreatedBy = createdBy,
            CreatedAt = utcNow,
            ExpiresAt = utcNow + validFor
        };

        return (invite, InviteToken.Format(key, secret));
    }

    /// <summary>Creates the invite for the first administrator account, with the token for its link.</summary>
    public static (Invite Invite, string Token) CreateFirstAdmin(TimeSpan validFor, DateTime utcNow)
    {
        EnsureValidValidity(validFor);

        var (secret, hash) = InviteToken.NewSecret();
        var invite = new Invite
        {
            AggregateKey = Invite.FirstAdminKey,
            SecretHash = hash,
            Note = "First administrator",
            GrantsAdmin = true,
            CreatedBy = "system",
            CreatedAt = utcNow,
            ExpiresAt = utcNow + validFor
        };

        return (invite, InviteToken.Format(invite.AggregateKey, secret));
    }

    /// <summary>
    /// Gives the first administrator's invite a new secret and expiry, so earlier links stop working. Allowed for as
    /// long as there is no administrator, even if the invite was used (by an account that was removed since).
    /// </summary>
    public static string ReissueFirstAdmin(this Invite invite, TimeSpan validFor, DateTime utcNow)
    {
        if (invite.AggregateKey != Invite.FirstAdminKey)
            throw new InvalidOperationException("Only the first administrator's invite can be reissued.");
        EnsureValidValidity(validFor);

        var (secret, hash) = InviteToken.NewSecret();
        invite.SecretHash = hash;
        invite.ExpiresAt = utcNow + validFor;
        invite.RevokedAt = null;
        invite.RedeemedAt = null;
        invite.RedeemedBy = null;

        return InviteToken.Format(invite.AggregateKey, secret);
    }

    public static InviteStatus StatusAt(this Invite invite, DateTime utcNow) =>
        invite.RedeemedAt is not null ? InviteStatus.Used
        : invite.RevokedAt is not null ? InviteStatus.Revoked
        : invite.ExpiresAt <= utcNow ? InviteStatus.Expired
        : InviteStatus.Open;

    /// <summary>Whether the secret belongs to this invite and the invite can still be used.</summary>
    public static bool CanBeRedeemedWith(this Invite invite, string secret, DateTime utcNow) =>
        invite.StatusAt(utcNow) == InviteStatus.Open && InviteToken.Matches(secret, invite.SecretHash);

    /// <summary>Uses up the invite for the account that was created with it.</summary>
    public static void Redeem(this Invite invite, string secret, string redeemedBy, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redeemedBy);
        if (!invite.CanBeRedeemedWith(secret, utcNow))
            throw new InvalidOperationException("This invite link is invalid, has expired or has already been used.");

        invite.RedeemedAt = utcNow;
        invite.RedeemedBy = redeemedBy;
    }

    /// <summary>Makes an open invite's link stop working.</summary>
    public static void Revoke(this Invite invite, DateTime utcNow)
    {
        var status = invite.StatusAt(utcNow);
        if (status != InviteStatus.Open)
            throw new InvalidOperationException($"Only open invites can be revoked; this one is {status.ToString().ToLowerInvariant()}.");

        invite.RevokedAt = utcNow;
    }

    private static void EnsureValidValidity(TimeSpan validFor)
    {
        if (validFor <= TimeSpan.Zero || validFor > Invite.MaxValidity)
            throw new ArgumentOutOfRangeException(nameof(validFor), validFor,
                $"An invite must be valid for some time, up to {Invite.MaxValidity.TotalDays} days.");
    }
}
