namespace MyGameCollection.Application.Abstractions.Queries.GetInvites;

public record InviteDto
{
    public required int Id { get; init; }
    public string? Note { get; init; }
    public bool GrantsAdmin { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required InviteStatus Status { get; init; }

    /// <summary>The user name of the account that was created with the invite.</summary>
    public string? RedeemedBy { get; init; }
}

public enum InviteStatus
{
    Open,
    Used,
    Expired,
    Revoked
}
