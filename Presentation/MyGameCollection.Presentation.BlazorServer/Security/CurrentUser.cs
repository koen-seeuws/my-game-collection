using System.Security.Claims;
using MyGameCollection.Infrastructure.Identity;

namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// The user signed in on the current circuit (one per browser tab), kept up to date by
/// <see cref="UserCircuitHandler"/> from the authentication state of the circuit.
/// </summary>
public sealed class CurrentUser
{
    /// <summary>The account id, which owns the user's game collection.</summary>
    public string? UserId { get; private set; }

    /// <summary>First and last name, or the email address for an account without a name.</summary>
    public string? DisplayName { get; private set; }

    public bool IsSignedIn => UserId is not null;

    public bool IsAdmin { get; private set; }

    /// <summary>Raised when the user signs in or out (e.g. when the account is removed).</summary>
    public event Action? Changed;

    internal void Set(ClaimsPrincipal principal)
    {
        var userId = principal.Identity?.IsAuthenticated == true
            ? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var fullName = $"{principal.FindFirstValue(ClaimTypes.GivenName)} {principal.FindFirstValue(ClaimTypes.Surname)}".Trim();
        var displayName = userId is null ? null : fullName.Length > 0 ? fullName : principal.Identity?.Name;
        var isAdmin = userId is not null && principal.IsInRole(AccountRoles.Admin);
        if (userId == UserId && displayName == DisplayName && isAdmin == IsAdmin)
            return;

        UserId = userId;
        DisplayName = displayName;
        IsAdmin = isAdmin;
        Changed?.Invoke();
    }
}
