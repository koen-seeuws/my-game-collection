using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Business;
using MyGameCollection.Infrastructure.EFC;
using MyGameCollection.Infrastructure.Identity;
using MyGameCollection.Infrastructure.Igdb;
using MyGameCollection.Presentation.BlazorServer.Components;
using MyGameCollection.Presentation.BlazorServer.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGameCollectionApplicationServices();

var connectionString = builder.Configuration.GetConnectionString("Database");
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSqlLitePersistenceServices(builder.Configuration);
    builder.Services.AddAccountServices(options => options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddPostgresPersistenceServices(builder.Configuration);
    builder.Services.AddAccountServices(options => options.UseNpgsql(connectionString));
}

// Prefilling the add game / console forms from an IGDB search.
builder.Services.AddIgdbServices(builder.Configuration);

// Who is signed in, per circuit (browser tab): taken from the login cookie and re-checked every few minutes.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<CircuitHandler, UserCircuitHandler>();

// Lets the use-case scopes created by the dispatcher reach the circuit's CurrentUser.
builder.Services.AddSingleton<CircuitServicesAccessor>();
builder.Services.AddScoped<CircuitHandler, CircuitServicesAccessorHandler>();
builder.Services.AddScoped<IUserContext, CircuitUserContext>();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // No migrations exist yet: create the schema on startup for local development.
    using var scope = app.Services.CreateScope();
    // The game tables first: EnsureCreated does nothing once the database has any table.
    scope.ServiceProvider.GetRequiredService<GameCollectionDbContext>().Database.EnsureCreated();
    await app.Services.EnsureAccountTablesCreatedAsync();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Prices and dates are shown in this culture (euro, day/month/year by default).
var culture = app.Configuration["Culture"] ?? "en-BE";
app.UseRequestLocalization(options => options
    .SetDefaultCulture(culture)
    .AddSupportedCultures(culture)
    .AddSupportedUICultures(culture));

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Binding a form field makes the endpoint validate the antiforgery token of the sign-out form.
app.MapPost("/account/logout", async (SignInManager<AppUser> signInManager, [FromForm] string? returnUrl) =>
{
    await signInManager.SignOutAsync();
    return TypedResults.LocalRedirect($"~{AccountPaths.Login}");
});

await LogFirstAdminInviteAsync(app);

app.Run();

// Until somebody is an administrator, log a one-time link to create the administrator account.
static async Task LogFirstAdminInviteAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var token = await scope.ServiceProvider.GetRequiredService<InviteService>()
        .CreateFirstAdminInviteIfNeededAsync(TimeSpan.FromDays(1));
    if (token is null)
        return;

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var baseUrl = app.Urls.FirstOrDefault()?.TrimEnd('/') ?? string.Empty;
        app.Logger.LogWarning(
            "There is no administrator yet. Create the administrator account within 24 hours at: {InviteUrl}",
            $"{baseUrl}/account/register/{token}");
    });
}
