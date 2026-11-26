using Microsoft.AspNetCore.Components.Server.Circuits;

namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// Gives access to the services of the circuit that is currently handling a UI event — the Blazor
/// counterpart of <c>IHttpContextAccessor</c>. Works from any DI scope, including the per-use-case
/// scopes the dispatcher creates.
/// </summary>
/// <remarks>
/// See "Access server-side Blazor services from a different DI scope" in the ASP.NET Core docs.
/// </remarks>
public sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> CircuitServices = new();

    public IServiceProvider? Services
    {
        get => CircuitServices.Value;
        set => CircuitServices.Value = value;
    }
}

/// <summary>Sets <see cref="CircuitServicesAccessor.Services"/> around every inbound circuit activity.</summary>
public sealed class CircuitServicesAccessorHandler : CircuitHandler
{
    private readonly IServiceProvider _services;
    private readonly CircuitServicesAccessor _accessor;

    public CircuitServicesAccessorHandler(IServiceProvider services, CircuitServicesAccessor accessor)
    {
        _services = services;
        _accessor = accessor;
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            _accessor.Services = _services;
            await next(context);
            _accessor.Services = null;
        };
}
