using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Queries.GetUsers;

namespace MyGameCollection.Infrastructure.Identity.QueryHandlers.GetUsers;

public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly AccountsDbContext _db;
    private readonly IUserContext _userContext;

    public GetUsersQueryHandler(AccountsDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery query, CancellationToken ct = default)
    {
        _userContext.EnsureAdmin();

        var adminIds = _db.UserRoles
            .Where(ur => _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == AccountRoles.Admin))
            .Select(ur => ur.UserId);

        return await _db.Users.AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Select(u => new UserDto(u.FirstName, u.LastName, u.Email!, adminIds.Contains(u.Id), u.CreatedAt))
            .ToListAsync(ct);
    }
}
