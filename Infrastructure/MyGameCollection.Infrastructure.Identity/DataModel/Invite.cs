namespace MyGameCollection.Infrastructure.Identity.DataModel;

/// <summary>Stores a <see cref="Core.Domain.Invite"/>.</summary>
public class Invite
{
    public int Id { get; set; }

    /// <summary>The domain key of the aggregate (unique).</summary>
    public Guid Key { get; set; }

    public required string SecretHash { get; set; }
    public string? Note { get; set; }
    public bool GrantsAdmin { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public string? RedeemedBy { get; set; }

    /// <summary>Raised on every save: a save based on an outdated read fails (e.g. one invite redeemed twice).</summary>
    public int Version { get; set; }
}
