using Microsoft.AspNetCore.Identity;
using MyGameCollection.Application.Abstractions.Services.Accounts;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>Accounts with ASP.NET Core Identity, in <see cref="AccountsDbContext"/>.</summary>
internal sealed class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TimeProvider _time;

    public AccountService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, TimeProvider time)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _time = time;
    }

    public async Task<CreateAccountResult> CreateAccountAsync(string firstName, string lastName, string email,
        string password, bool isAdmin, CancellationToken cancellationToken = default)
    {
        // Identity needs a user name: the email address doubles as one.
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = _time.GetUtcNow().UtcDateTime
        };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return new CreateAccountResult(null, created.Errors
                .Where(e => e.Code is not (nameof(IdentityErrorDescriber.DuplicateUserName)
                    or nameof(IdentityErrorDescriber.InvalidUserName)))
                .Select(e => e.Description)
                .DefaultIfEmpty("The account could not be created.")
                .ToArray());

        if (isAdmin)
        {
            if (!await _roleManager.RoleExistsAsync(AccountRoles.Admin))
                EnsureSucceeded(await _roleManager.CreateAsync(new IdentityRole(AccountRoles.Admin)));

            EnsureSucceeded(await _userManager.AddToRoleAsync(user, AccountRoles.Admin));
        }

        return new CreateAccountResult(user.Id, []);
    }

    public async Task<bool> HasAdminAsync(CancellationToken cancellationToken = default) =>
        (await _userManager.GetUsersInRoleAsync(AccountRoles.Admin)).Count > 0;

    // Unexpected here (the account itself was valid): throwing rolls back the whole use case.
    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
