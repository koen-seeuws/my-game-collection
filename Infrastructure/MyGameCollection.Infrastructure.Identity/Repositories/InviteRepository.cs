using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.Persistence;
using Domain = MyGameCollection.Core.Domain;

namespace MyGameCollection.Infrastructure.Identity.Repositories;

public sealed class InviteRepository : IRepository<Domain.Invite, Guid>
{
    private readonly AccountsDbContext _db;

    public InviteRepository(AccountsDbContext db)
    {
        _db = db;
    }

    public async Task<Domain.Invite?> GetAsync(Guid key, CancellationToken ct = default)
    {
        var row = await _db.Invites.SingleOrDefaultAsync(i => i.Key == key, ct);
        return row is null ? null : ToAggregate(row);
    }

    public async Task SaveAsync(Domain.Invite invite, CancellationToken ct = default)
    {
        var row = await _db.Invites.SingleOrDefaultAsync(i => i.Key == invite.AggregateKey, ct);
        if (row is null)
        {
            row = new DataModel.Invite { Key = invite.AggregateKey, SecretHash = invite.SecretHash, CreatedBy = invite.CreatedBy };
            _db.Invites.Add(row);
        }

        row.SecretHash = invite.SecretHash;
        row.Note = invite.Note;
        row.GrantsAdmin = invite.GrantsAdmin;
        row.CreatedBy = invite.CreatedBy;
        row.CreatedAt = invite.CreatedAt;
        row.ExpiresAt = invite.ExpiresAt;
        row.RevokedAt = invite.RevokedAt;
        row.RedeemedAt = invite.RedeemedAt;
        row.RedeemedBy = invite.RedeemedBy;
        row.Version++;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("This invite was just used or changed by someone else. Try again.");
        }
    }

    internal static Domain.Invite ToAggregate(DataModel.Invite row) => new()
    {
        AggregateKey = row.Key,
        SecretHash = row.SecretHash,
        Note = row.Note,
        GrantsAdmin = row.GrantsAdmin,
        CreatedBy = row.CreatedBy,
        CreatedAt = row.CreatedAt,
        ExpiresAt = row.ExpiresAt,
        RevokedAt = row.RevokedAt,
        RedeemedAt = row.RedeemedAt,
        RedeemedBy = row.RedeemedBy
    };
}
