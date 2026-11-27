using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetOpenInvite;

namespace MyGameCollection.Infrastructure.Identity.QueryHandlers.GetOpenInvite;

/// <summary>Runs for someone who is not signed in yet: the invite token is the authorization.</summary>
public sealed class GetOpenInviteQueryHandler : IQueryHandler<GetOpenInviteQuery, OpenInviteDto?>
{
    private readonly AccountsDbContext _db;
    private readonly TimeProvider _time;

    public GetOpenInviteQueryHandler(AccountsDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public Task<OpenInviteDto?> HandleAsync(GetOpenInviteQuery query, CancellationToken ct = default)
    {
        var hash = Invite.HashToken(query.Token);
        var now = _time.GetUtcNow().UtcDateTime;

        return _db.Invites
            .Where(i => i.TokenHash == hash && i.RedeemedAt == null && i.RevokedAt == null && i.ExpiresAt > now)
            .Select(i => new OpenInviteDto(i.GrantsAdmin))
            .SingleOrDefaultAsync(ct);
    }
}
