using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;
using MyGameCollection.Application.Abstractions.Services.Accounts;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>Accounts and invites with ASP.NET Core Identity, in <see cref="AccountsDbContext"/>.</summary>
internal sealed class AccountService : IAccountService
{
    private readonly AccountsDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TimeProvider _time;

    public AccountService(AccountsDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        TimeProvider time)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _time = time;
    }

    private DateTime UtcNow => _time.GetUtcNow().UtcDateTime;

    public async Task<CreatedInviteDto> CreateInviteAsync(string createdBy, string? note, TimeSpan validFor,
        bool grantsAdmin, CancellationToken cancellationToken = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = UtcNow;
        var invite = new Invite
        {
            TokenHash = Invite.HashToken(token),
            Note = note,
            GrantsAdmin = grantsAdmin,
            CreatedBy = createdBy,
            CreatedAt = now,
            ExpiresAt = now + validFor
        };

        _db.Invites.Add(invite);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreatedInviteDto(token, invite.ExpiresAt);
    }

    public async Task<bool> RevokeInviteAsync(int id, CancellationToken cancellationToken = default)
    {
        var now = UtcNow;

        return await _db.Invites
            .Where(i => i.Id == id && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.RevokedAt, now), cancellationToken) == 1;
    }

    public async Task RevokeOpenAdminInvitesAsync(CancellationToken cancellationToken = default)
    {
        var now = UtcNow;

        await _db.Invites
            .Where(i => i.GrantsAdmin && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.RevokedAt, now), cancellationToken);
    }

    public async Task<RedeemInviteResultDto> RedeemInviteAsync(string token, string userName, string email,
        string password, CancellationToken cancellationToken = default)
    {
        // The account and the used invite are saved together, or not at all.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var hash = Invite.HashToken(token);
        var now = UtcNow;
        var invite = await _db.Invites.AsNoTracking().SingleOrDefaultAsync(
            i => i.TokenHash == hash && i.RedeemedAt == null && i.RevokedAt == null && i.ExpiresAt > now,
            cancellationToken);
        if (invite is null)
            return RedeemInviteResultDto.Failure("This invite link is invalid, has expired or has already been used.");

        var user = new AppUser { UserName = userName, Email = email, CreatedAt = now };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return RedeemInviteResultDto.Failure(created.Errors.Select(e => e.Description).ToArray());

        if (invite.GrantsAdmin)
        {
            if (!await _roleManager.RoleExistsAsync(AccountRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(AccountRoles.Admin));

            var addedToRole = await _userManager.AddToRoleAsync(user, AccountRoles.Admin);
            if (!addedToRole.Succeeded)
                return RedeemInviteResultDto.Failure(addedToRole.Errors.Select(e => e.Description).ToArray());
        }

        // Claim the invite only if nobody else did in the meantime.
        var claimed = await _db.Invites
            .Where(i => i.Id == invite.Id && i.RedeemedAt == null && i.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.RedeemedAt, now)
                .SetProperty(i => i.RedeemedBy, user.UserName), cancellationToken);
        if (claimed != 1)
            return RedeemInviteResultDto.Failure("This invite link has already been used.");

        await transaction.CommitAsync(cancellationToken);
        return RedeemInviteResultDto.Success(user.UserName);
    }

    public async Task<bool> HasAdminAsync(CancellationToken cancellationToken = default) =>
        (await _userManager.GetUsersInRoleAsync(AccountRoles.Admin)).Count > 0;
}
