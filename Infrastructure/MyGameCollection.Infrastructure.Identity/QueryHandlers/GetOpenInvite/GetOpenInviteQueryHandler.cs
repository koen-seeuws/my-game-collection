using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetOpenInvite;
using MyGameCollection.Core.Domain;
using MyGameCollection.Infrastructure.Identity.Repositories;

namespace MyGameCollection.Infrastructure.Identity.QueryHandlers.GetOpenInvite;

/// <summary>Runs for someone who is not signed in yet: the invite token is the authorization.</summary>
public sealed class GetOpenInviteQueryHandler : IQueryHandler<GetOpenInviteQuery, OpenInviteDto?>
{
    private readonly InviteRepository _invites;
    private readonly TimeProvider _time;

    public GetOpenInviteQueryHandler(InviteRepository invites, TimeProvider time)
    {
        _invites = invites;
        _time = time;
    }

    public async Task<OpenInviteDto?> HandleAsync(GetOpenInviteQuery query, CancellationToken ct = default)
    {
        if (!InviteToken.TryParse(query.Token, out var key, out var secret)
            || await _invites.GetAsync(key, ct) is not { } invite
            || !invite.CanBeRedeemedWith(secret, _time.GetUtcNow().UtcDateTime))
            return null;

        return new OpenInviteDto(invite.GrantsAdmin);
    }
}
