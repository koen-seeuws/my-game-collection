using System.Security.Claims;
using MyGameCollection.Infrastructure.Identity;

namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// The user signed in on the current circuit (one per browser tab), kept up to date by
/// <see cref="UserCircuitHandler"/> from the authentication state of the circuit.
/// </summary>
public sealed class CurrentUser
{
    public string? UserName { get; private set; }

    public bool IsSignedIn => UserName is not null;

    public bool IsAdmin { get; private set; }

    /// <summary>Raised when the user signs in or out (e.g. when the account is removed).</summary>
    public event Action? Changed;

    internal void Set(ClaimsPrincipal principal)
    {
        var userName = principal.Identity?.IsAuthenticated == true ? principal.Identity.Name : null;
        var isAdmin = userName is not null && principal.IsInRole(AccountRoles.Admin);
        if (userName == UserName && isAdmin == IsAdmin)
            return;

        UserName = userName;
        IsAdmin = isAdmin;
        Changed?.Invoke();
    }
}
