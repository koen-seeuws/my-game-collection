using System.Data.Common;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MyGameCollection.Infrastructure.Identity;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers cookie sign-in with ASP.NET Core Identity (accounts by invitation only), stored with the given
    /// database provider.
    /// </summary>
    public static IServiceCollection AddAccountServices(this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddDbContext<AccountsDbContext>(configureDatabase);

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Length over composition rules (NIST SP 800-63B).
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AccountsDbContext>()
            .AddSignInManager();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "MyGameCollection.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.LoginPath = AccountPaths.Login;
            options.AccessDeniedPath = "/";
        });

        // Keep the keys that protect the login cookie in the database, so restarts don't sign everybody out.
        services.AddDataProtection()
            .SetApplicationName("MyGameCollection")
            .PersistKeysToDbContext<AccountsDbContext>();

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<InviteService>();

        return services;
    }

    /// <summary>
    /// Creates the account tables when they don't exist yet. The game collection tables share the database, so
    /// <c>EnsureCreated</c> alone would skip them once the database exists.
    /// </summary>
    public static async Task EnsureAccountTablesCreatedAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        var creator = db.GetService<IRelationalDatabaseCreator>();

        if (!await creator.ExistsAsync(ct))
            await creator.CreateAsync(ct);

        try
        {
            await db.Invites.AnyAsync(ct);
        }
        catch (DbException)
        {
            await creator.CreateTablesAsync(ct);
        }
    }
}
