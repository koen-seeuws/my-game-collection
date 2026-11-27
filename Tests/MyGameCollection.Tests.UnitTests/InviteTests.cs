using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;
using MyGameCollection.Application.Abstractions.Commands.RevokeInvite;
using MyGameCollection.Application.Abstractions.Queries.GetInvites;
using MyGameCollection.Application.Abstractions.Queries.GetOpenInvite;
using MyGameCollection.Application.Abstractions.Queries.GetUsers;
using MyGameCollection.Application.Business;
using MyGameCollection.Infrastructure.Identity;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>The invite use cases through the dispatcher, on an in-memory SQLite database.</summary>
public sealed class InviteTests : IAsyncLifetime
{
    private const string Password = "a long enough password";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeUserContext _user = new() { UserName = "admin", IsAdmin = true };
    private ServiceProvider _services = default!;

    private IUseCaseDispatcher Dispatcher => _services.GetRequiredService<IUseCaseDispatcher>();

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_time);
        services.AddSingleton<IUserContext>(_user);
        services.AddGameCollectionApplicationServices();
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

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Null(await FindUserAsync("bob"));

        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));
        Assert.Equal(InviteStatus.Used, invite.Status);
        Assert.Equal("alice", invite.RedeemedBy);
        Assert.Equal("admin", invite.CreatedBy);
    }

    [Fact]
    public async Task An_expired_invite_cannot_be_used()
    {
        var token = await CreateInviteAsync(TimeSpan.FromDays(1));
        _time.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1));

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }));
        Assert.False((await RedeemAsync(token, "alice", "alice@example.com")).Succeeded);
        Assert.Equal(InviteStatus.Expired, Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery())).Status);
    }

    [Fact]
    public async Task A_revoked_invite_cannot_be_used_or_revoked_again()
    {
        var token = await CreateInviteAsync();
        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));

        await Dispatcher.SendAsync(new RevokeInviteCommand { Id = invite.Id });

        Assert.False((await RedeemAsync(token, "alice", "alice@example.com")).Succeeded);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Dispatcher.SendAsync(new RevokeInviteCommand { Id = invite.Id }));
    }

    [Fact]
    public async Task A_failed_registration_leaves_the_invite_usable()
    {
        var token = await CreateInviteAsync();

        var tooShort = await Dispatcher.SendAsync(new RedeemInviteCommand
        {
            Token = token, UserName = "alice", Email = "alice@example.com", Password = "short"
        });
        var retried = await RedeemAsync(token, "alice", "alice@example.com");

        Assert.False(tooShort.Succeeded);
        Assert.NotEmpty(tooShort.Errors);
        Assert.True(retried.Succeeded);
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        await CreateInviteAsync();

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = "not-a-real-token" }));
        Assert.False((await RedeemAsync("not-a-real-token", "alice", "alice@example.com")).Succeeded);
    }

    [Fact]
    public async Task Only_administrators_can_manage_invites_and_see_users()
    {
        var token = await CreateInviteAsync();
        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));
        _user.IsAdmin = false;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateInviteAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Dispatcher.SendAsync(new RevokeInviteCommand { Id = invite.Id }));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Dispatcher.QueryAsync(new GetInvitesQuery()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Dispatcher.QueryAsync(new GetUsersQuery()));

        // Opening and using a link needs no signed-in user at all.
        Assert.NotNull(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }));
        Assert.True((await RedeemAsync(token, "alice", "alice@example.com")).Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public async Task An_invite_must_be_valid_for_up_to_90_days(int days)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateInviteAsync(TimeSpan.FromDays(days)));
    }

    [Fact]
    public async Task The_first_admin_invite_makes_an_administrator_and_is_not_offered_again()
    {
        var token = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });
        Assert.NotNull(token);
        Assert.True((await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }))!.GrantsAdmin);

        Assert.True((await RedeemAsync(token, "owner", "owner@example.com")).Succeeded);

        var owner = Assert.Single(await Dispatcher.QueryAsync(new GetUsersQuery()));
        Assert.Equal("owner", owner.UserName);
        Assert.True(owner.IsAdmin);
        Assert.Null(await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) }));
    }

    [Fact]
    public async Task A_new_first_admin_invite_revokes_the_previous_one()
    {
        var first = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });
        var second = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = first! }));
        Assert.NotNull(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = second! }));
    }

    [Fact]
    public async Task A_normal_invite_does_not_make_an_administrator()
    {
        var token = await CreateInviteAsync();
        await RedeemAsync(token, "alice", "alice@example.com");

        var alice = Assert.Single(await Dispatcher.QueryAsync(new GetUsersQuery()));
        Assert.False(alice.IsAdmin);
    }

    private async Task<string> CreateInviteAsync(TimeSpan? validFor = null) =>
        (await Dispatcher.SendAsync(new CreateInviteCommand { Note = "test", ValidFor = validFor ?? TimeSpan.FromDays(7) }))
        .Token;

    private Task<RedeemInviteResultDto> RedeemAsync(string token, string userName, string email) =>
        Dispatcher.SendAsync(new RedeemInviteCommand
        {
            Token = token, UserName = userName, Email = email, Password = Password
        });

    private async Task<AppUser?> FindUserAsync(string userName)
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>().FindByNameAsync(userName);
    }

    private sealed class FakeUserContext : IUserContext
    {
        public required string UserName { get; init; }
        public bool IsAdmin { get; set; }
    }

    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
