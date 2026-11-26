using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor.Services;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Business;
using MyGameCollection.Infrastructure.EFC;
using MyGameCollection.Presentation.BlazorServer.Components;
using MyGameCollection.Presentation.BlazorServer.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGameCollectionApplicationServices();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSqlLitePersistenceServices(builder.Configuration);
}
else
{
    builder.Services.AddPostgresPersistenceServices(builder.Configuration);
}

// Who is signed in, per circuit (browser tab).
builder.Services.AddScoped<CurrentUser>();

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
    scope.ServiceProvider.GetRequiredService<GameCollectionDbContext>().Database.EnsureCreated();
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
