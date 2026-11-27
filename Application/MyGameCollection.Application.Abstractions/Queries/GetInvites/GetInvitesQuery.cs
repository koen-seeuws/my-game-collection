namespace MyGameCollection.Application.Abstractions.Queries.GetInvites;

/// <summary>All invites, newest first (administrators only).</summary>
public record GetInvitesQuery : IQuery<IReadOnlyList<InviteDto>>;
