using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using MyGameCollection.Application;
using MyGameCollection.Infrastructure.Barcodes;
using MyGameCollection.Infrastructure.Metadata;
using MyGameCollection.Infrastructure.Persistence;
using MyGameCollection.Infrastructure.Storage;
using MyGameCollection.Web.Components;
using MyGameCollection.Web.Endpoints;
using MyGameCollection.Web.Services;

// `dotnet MyGameCollection.Web.dll --healthcheck` is used by the Docker HEALTHCHECK (the runtime image has no curl).
if (args.Contains("--healthcheck"))
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    var port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")?.Split(';')[0] ?? "8080";
    try { return (await http.GetAsync($"http://localhost:{port}/health")).IsSuccessStatusCode ? 0 : 1; }
    catch { return 1; }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
});

builder.Services.AddApplication(builder.Configuration["MediatR:LicenseKey"]);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddLocalPhotoStorage(builder.Configuration);
builder.Services.AddBarcodeLookup();
builder.Services.AddRawgMetadata(builder.Configuration);

builder.Services.AddScoped<CollectionFilterState>();
builder.Services.AddScoped<ThemeState>();

// Persist data protection keys (antiforgery, circuits) so restarts of the container don't break open sessions.
builder.Services.AddDataProtection()
    .SetApplicationName("MyGameCollection")
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["DataProtection:KeysPath"] ?? "data/keys"));

// The app usually runs behind a reverse proxy (Caddy / Traefik / Nginx Proxy Manager) on the home network.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapPhotoEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
return 0;
