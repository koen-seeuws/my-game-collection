using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetInvites;

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
        var invites = await _db.Invites.AsNoTracking().OrderByDescending(i => i.Id).ToListAsync(ct);

        return invites.Select(i => new InviteDto
            {
                Id = i.Id,
                Note = i.Note,
                GrantsAdmin = i.GrantsAdmin,
                CreatedBy = i.CreatedBy,
                CreatedAt = i.CreatedAt,
                ExpiresAt = i.ExpiresAt,
                Status = i.GetStatus(now),
                RedeemedBy = i.RedeemedBy
            })
            .ToList();
    }
}
