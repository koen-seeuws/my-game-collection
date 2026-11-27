using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetInvites;
using MyGameCollection.Core.Domain;
using MyGameCollection.Infrastructure.Identity.Repositories;

namespace MyGameCollection.Infrastructure.Identity.QueryHandlers.GetInvites;

public sealed class GetInvitesQueryHandler : IQueryHandler<GetInvitesQuery, IReadOnlyList<InviteDto>>
{
    private readonly AccountsDbContext _db;
    private readonly IUserContext _userContext;
    private readonly TimeProvider _time;

    public GetInvitesQueryHandler(AccountsDbContext db, IUserContext userContext, TimeProvider time)
    {
        _db = db;
        _userContext = userContext;
        _time = time;
    }

    public async Task<IReadOnlyList<InviteDto>> HandleAsync(GetInvitesQuery query, CancellationToken ct = default)
    {
        _userContext.EnsureAdmin();

        var now = _time.GetUtcNow().UtcDateTime;
        var rows = await _db.Invites.AsNoTracking().OrderByDescending(i => i.CreatedAt).ToListAsync(ct);

        // The status is a domain rule: map to the aggregate rather than repeating it here.
        return rows.Select(InviteRepository.ToAggregate)
            .Select(i => new InviteDto
            {
                Key = i.AggregateKey,
                Note = i.Note,
                GrantsAdmin = i.GrantsAdmin,
                CreatedBy = i.CreatedBy,
                CreatedAt = i.CreatedAt,
                ExpiresAt = i.ExpiresAt,
                Status = i.StatusAt(now),
                RedeemedBy = i.RedeemedBy
            })
            .ToList();
    }
}
