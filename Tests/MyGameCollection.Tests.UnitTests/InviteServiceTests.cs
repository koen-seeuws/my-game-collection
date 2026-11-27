using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Infrastructure.Identity;

namespace MyGameCollection.Tests.UnitTests;

public sealed class InviteServiceTests : IAsyncLifetime
{
    private const string Password = "a long enough password";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero));
    private ServiceProvider _services = default!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_time);
        services.AddAccountServices(options => options.UseSqlite(_connection));
        _services = services.BuildServiceProvider();

        await _services.EnsureAccountTablesCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _services.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task An_invite_creates_exactly_one_account()
    {
        var token = await CreateInviteAsync();

        var first = await RedeemAsync(token, "alice", "alice@example.com");
        var second = await RedeemAsync(token, "bob", "bob@example.com");

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Null(await FindUserAsync("bob"));

        var invite = Assert.Single(await InScope(s => s.GetAllAsync()));
        Assert.Equal(InviteStatus.Used, invite.GetStatus(_time.GetUtcNow().UtcDateTime));
        Assert.Equal("alice", invite.RedeemedBy);
    }

    [Fact]
    public async Task An_expired_invite_cannot_be_used()
    {
        var token = await CreateInviteAsync(TimeSpan.FromDays(1));
        _time.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1));

        Assert.False((await RedeemAsync(token, "alice", "alice@example.com")).IsSuccess);
        Assert.Null(await FindUserAsync("alice"));
    }

    [Fact]
    public async Task A_revoked_invite_cannot_be_used()
    {
        var token = await CreateInviteAsync();
        var invite = Assert.Single(await InScope(s => s.GetAllAsync()));

        Assert.True(await InScope(s => s.RevokeAsync(invite.Id)));
        Assert.False((await RedeemAsync(token, "alice", "alice@example.com")).IsSuccess);
    }

    [Fact]
    public async Task A_failed_registration_leaves_the_invite_usable()
    {
        var token = await CreateInviteAsync();

        var tooShort = await InScope(s => s.RedeemAsync(token, "alice", "alice@example.com", "short"));
        var retried = await RedeemAsync(token, "alice", "alice@example.com");

        Assert.False(tooShort.IsSuccess);
        Assert.NotEmpty(tooShort.Errors);
        Assert.True(retried.IsSuccess);
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        await CreateInviteAsync();

        Assert.Null(await InScope(s => s.FindOpenAsync("not-a-real-token")));
        Assert.False((await RedeemAsync("not-a-real-token", "alice", "alice@example.com")).IsSuccess);
    }

    [Fact]
    public async Task The_first_admin_invite_makes_an_administrator_and_is_not_offered_again()
    {
        var token = await InScope(s => s.CreateFirstAdminInviteIfNeededAsync(TimeSpan.FromDays(1)));
        Assert.NotNull(token);

        var result = await RedeemAsync(token, "owner", "owner@example.com");

        Assert.True(result.IsSuccess);
        await using (var scope = _services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            Assert.True(await userManager.IsInRoleAsync((await userManager.FindByNameAsync("owner"))!, InviteService.AdminRole));
        }

        Assert.Null(await InScope(s => s.CreateFirstAdminInviteIfNeededAsync(TimeSpan.FromDays(1))));
    }

    [Fact]
    public async Task A_new_first_admin_invite_revokes_the_previous_one()
    {
        var first = await InScope(s => s.CreateFirstAdminInviteIfNeededAsync(TimeSpan.FromDays(1)));
        var second = await InScope(s => s.CreateFirstAdminInviteIfNeededAsync(TimeSpan.FromDays(1)));

        Assert.Null(await InScope(s => s.FindOpenAsync(first!)));
        Assert.NotNull(await InScope(s => s.FindOpenAsync(second!)));
    }

    [Fact]
    public async Task A_normal_invite_does_not_make_an_administrator()
    {
        var token = await CreateInviteAsync();
        await RedeemAsync(token, "alice", "alice@example.com");

        await using var scope = _services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        Assert.False(await userManager.IsInRoleAsync((await userManager.FindByNameAsync("alice"))!, InviteService.AdminRole));
    }

    private Task<string> CreateInviteAsync(TimeSpan? validFor = null) =>
        InScope(async s => (await s.CreateAsync("admin", "test", validFor ?? TimeSpan.FromDays(7))).Token);

    private Task<RedeemInviteResult> RedeemAsync(string token, string userName, string email) =>
        InScope(s => s.RedeemAsync(token, userName, email, Password));

    private async Task<AppUser?> FindUserAsync(string userName)
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>().FindByNameAsync(userName);
    }

    private async Task<T> InScope<T>(Func<InviteService, Task<T>> action)
    {
        await using var scope = _services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<InviteService>());
    }

    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
