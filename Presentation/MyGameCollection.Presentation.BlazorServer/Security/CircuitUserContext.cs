using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// Resolves the current user from the <see cref="CurrentUser"/> of the active circuit — the Blazor
/// counterpart of the Web API's <c>HttpUserContext</c>.
/// </summary>
public sealed class CircuitUserContext : IUserContext
{
    private readonly CircuitServicesAccessor _circuitServicesAccessor;

    public CircuitUserContext(CircuitServicesAccessor circuitServicesAccessor)
    {
        _circuitServicesAccessor = circuitServicesAccessor;
    }

    public string UserId =>
        CurrentUser.UserId ?? throw new UnauthorizedAccessException("Sign in before working with your game collection.");

    public bool IsAdmin => CurrentUser.IsAdmin;

    private CurrentUser CurrentUser
    {
        get
        {
            var circuitServices = _circuitServicesAccessor.Services
                ?? throw new InvalidOperationException("There is no active Blazor circuit.");

            return circuitServices.GetRequiredService<CurrentUser>();
        }
    }
}
