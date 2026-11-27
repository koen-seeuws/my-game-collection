namespace MyGameCollection.Application.Abstractions.Queries.GetOpenInvite;

/// <summary>The invite behind a link, if it can still be used to create an account. Needs no signed-in user.</summary>
public record GetOpenInviteQuery : IQuery<OpenInviteDto?>
{
    public required string Token { get; init; }
}

public record OpenInviteDto(bool GrantsAdmin);
