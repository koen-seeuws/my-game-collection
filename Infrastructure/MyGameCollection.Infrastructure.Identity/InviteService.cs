using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>Creates, lists, revokes and redeems invite links.</summary>
public sealed class InviteService
{
    public const string AdminRole = "Admin";

    private readonly AccountsDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TimeProvider _time;

    public InviteService(AccountsDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        TimeProvider time)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _time = time;
    }

    private DateTime UtcNow => _time.GetUtcNow().UtcDateTime;

    /// <summary>Creates an invite; the returned token goes into the link and cannot be retrieved later.</summary>
    public async Task<(Invite Invite, string Token)> CreateAsync(string createdBy, string? note, TimeSpan validFor,
        bool grantsAdmin = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);
        if (validFor <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(validFor), "An invite must be valid for some time.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = UtcNow;
        var invite = new Invite
        {
            TokenHash = Hash(token),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            GrantsAdmin = grantsAdmin,
            CreatedBy = createdBy,
            CreatedAt = now,
            ExpiresAt = now + validFor
        };

        _db.Invites.Add(invite);
        await _db.SaveChangesAsync(ct);

        return (invite, token);
    }

    public Task<List<Invite>> GetAllAsync(CancellationToken ct = default) =>
        _db.Invites.AsNoTracking().OrderByDescending(i => i.Id).ToListAsync(ct);

    /// <summary>The invite behind a link, if it can still be used to create an account.</summary>
    public Task<Invite?> FindOpenAsync(string token, CancellationToken ct = default)
    {
        var hash = Hash(token);
        var now = UtcNow;

        return _db.Invites.AsNoTracking().SingleOrDefaultAsync(
            i => i.TokenHash == hash && i.RedeemedAt == null && i.RevokedAt == null && i.ExpiresAt > now, ct);
    }

    /// <summary>Revokes an invite that has not been used yet; returns whether it was revoked.</summary>
    public async Task<bool> RevokeAsync(int id, CancellationToken ct = default)
    {
        var now = UtcNow;

        return await _db.Invites
            .Where(i => i.Id == id && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.RevokedAt, now), ct) == 1;
    }

    /// <summary>Creates an account with an invite, which can then no longer be used.</summary>
    public async Task<RedeemInviteResult> RedeemAsync(string token, string userName, string email, string password,
        CancellationToken ct = default)
    {
        // The account and the used invite are saved together, or not at all.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var invite = await FindOpenAsync(token, ct);
        if (invite is null)
            return RedeemInviteResult.Failed("This invite link is invalid, has expired or has already been used.");

        var now = UtcNow;
        var user = new AppUser { UserName = userName.Trim(), Email = email.Trim(), CreatedAt = now };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return RedeemInviteResult.Failed(created.Errors.Select(e => e.Description).ToArray());

        if (invite.GrantsAdmin)
        {
            if (!await _roleManager.RoleExistsAsync(AdminRole))
                await _roleManager.CreateAsync(new IdentityRole(AdminRole));

            var addedToRole = await _userManager.AddToRoleAsync(user, AdminRole);
            if (!addedToRole.Succeeded)
                return RedeemInviteResult.Failed(addedToRole.Errors.Select(e => e.Description).ToArray());
        }

        // Claim the invite only if nobody else did in the meantime.
        var claimed = await _db.Invites
            .Where(i => i.Id == invite.Id && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.RedeemedAt, now)
                .SetProperty(i => i.RedeemedBy, user.UserName), ct);
        if (claimed != 1)
            return RedeemInviteResult.Failed("This invite link has already been used.");

        await transaction.CommitAsync(ct);
        return RedeemInviteResult.Succeeded(user);
    }

    /// <summary>
    /// When there is no administrator yet, replaces any earlier administrator invite with a new one and returns its
    /// token, so whoever runs the server can create the first (administrator) account.
    /// </summary>
    public async Task<string?> CreateFirstAdminInviteIfNeededAsync(TimeSpan validFor, CancellationToken ct = default)
    {
        if ((await _userManager.GetUsersInRoleAsync(AdminRole)).Count > 0)
            return null;

        var now = UtcNow;
        await _db.Invites
            .Where(i => i.GrantsAdmin && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.RevokedAt, now), ct);

        var (_, token) = await CreateAsync("system", "First administrator", validFor, grantsAdmin: true, ct);
        return token;
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public sealed record RedeemInviteResult(AppUser? User, IReadOnlyList<string> Errors)
{
    public bool IsSuccess => User is not null;

    public static RedeemInviteResult Succeeded(AppUser user) => new(user, []);
    public static RedeemInviteResult Failed(params string[] errors) => new(null, errors);
}
