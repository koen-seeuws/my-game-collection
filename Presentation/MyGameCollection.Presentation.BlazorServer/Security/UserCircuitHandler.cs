using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// Copies the circuit's authenticated user into <see cref="CurrentUser"/> before any component renders, and keeps it
/// in sync when the authentication state changes.
/// </summary>
/// <remarks>See "Circuit handler to capture users for custom services" in the ASP.NET Core docs.</remarks>
public sealed class UserCircuitHandler : CircuitHandler, IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly CurrentUser _currentUser;

    public UserCircuitHandler(AuthenticationStateProvider authenticationStateProvider, CurrentUser currentUser)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _currentUser = currentUser;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationChanged;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        _currentUser.Set(state.User);
    }

    private void OnAuthenticationChanged(Task<AuthenticationState> task) =>
        _ = UpdateAsync(task);

    private async Task UpdateAsync(Task<AuthenticationState> task)
    {
        var state = await task;
        _currentUser.Set(state.User);
    }

    public void Dispose() =>
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationChanged;
}
