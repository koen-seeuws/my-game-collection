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
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.Identity;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>The invite use cases through the dispatcher, on an in-memory SQLite database.</summary>
public sealed class InviteTests : IAsyncLifetime
{
    private const string Password = "a long enough password";

    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeUserContext _user = new() { UserId = "admin-id", IsAdmin = true };
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

        var first = await RedeemAsync(token, "alice@example.com");
        var second = await RedeemAsync(token, "bob@example.com");

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Null(await FindUserAsync("bob@example.com"));

        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));
        Assert.Equal(InviteStatus.Used, invite.Status);
        Assert.Equal("alice@example.com", invite.RedeemedBy);
        Assert.Equal("admin-id", invite.CreatedBy);
    }

    [Fact]
    public async Task An_expired_invite_cannot_be_used()
    {
        var token = await CreateInviteAsync(TimeSpan.FromDays(1));
        _time.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1));

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }));
        Assert.False((await RedeemAsync(token, "alice@example.com")).Succeeded);
        Assert.Equal(InviteStatus.Expired, Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery())).Status);
    }

    [Fact]
    public async Task A_revoked_invite_cannot_be_used_or_revoked_again()
    {
        var token = await CreateInviteAsync();
        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));

        await Dispatcher.SendAsync(new RevokeInviteCommand { Key = invite.Key });

        Assert.False((await RedeemAsync(token, "alice@example.com")).Succeeded);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Dispatcher.SendAsync(new RevokeInviteCommand { Key = invite.Key }));
    }

    [Fact]
    public async Task A_failed_registration_leaves_the_invite_usable()
    {
        var token = await CreateInviteAsync();

        var tooShort = await Dispatcher.SendAsync(new RedeemInviteCommand
        {
            Token = token, FirstName = "Alice", LastName = "Example", Email = "alice@example.com", Password = "short"
        });
        var retried = await RedeemAsync(token, "alice@example.com");

        Assert.False(tooShort.Succeeded);
        Assert.NotEmpty(tooShort.Errors);
        Assert.True(retried.Succeeded);
    }

    [Fact]
    public async Task A_link_with_the_wrong_secret_is_rejected()
    {
        var token = await CreateInviteAsync();
        var forged = token[..token.IndexOf('.')] + ".not-the-secret";

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = forged }));
        Assert.False((await RedeemAsync(forged, "alice@example.com")).Succeeded);
        Assert.True((await RedeemAsync(token, "alice@example.com")).Succeeded);
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        await CreateInviteAsync();

        Assert.Null(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = "not-a-real-token" }));
        Assert.False((await RedeemAsync("not-a-real-token", "alice@example.com")).Succeeded);
    }

    [Fact]
    public async Task Only_administrators_can_manage_invites_and_see_users()
    {
        var token = await CreateInviteAsync();
        var invite = Assert.Single(await Dispatcher.QueryAsync(new GetInvitesQuery()));
        _user.IsAdmin = false;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateInviteAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Dispatcher.SendAsync(new RevokeInviteCommand { Key = invite.Key }));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Dispatcher.QueryAsync(new GetInvitesQuery()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Dispatcher.QueryAsync(new GetUsersQuery()));

        // Opening and using a link needs no signed-in user at all.
        Assert.NotNull(await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }));
        Assert.True((await RedeemAsync(token, "alice@example.com")).Succeeded);
    }

    [Fact]
    public async Task An_email_address_can_only_have_one_account()
    {
        await RedeemAsync(await CreateInviteAsync(), "alice@example.com");

        var duplicate = await RedeemAsync(await CreateInviteAsync(), "ALICE@example.com");

        Assert.False(duplicate.Succeeded);
        Assert.Contains("taken", Assert.Single(duplicate.Errors));
    }

    [Fact]
    public async Task Names_are_required()
    {
        var result = await Dispatcher.SendAsync(new RedeemInviteCommand
        {
            Token = await CreateInviteAsync(), FirstName = " ", LastName = "User", Email = "a@example.com", Password = Password
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Redeeming_replies_with_the_new_account_id()
    {
        var result = await RedeemAsync(await CreateInviteAsync(), "alice@example.com");

        Assert.Equal((await FindUserAsync("alice@example.com"))!.Id, result.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public async Task An_invite_must_be_valid_for_up_to_90_days(int days)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => CreateInviteAsync(TimeSpan.FromDays(days)));
    }

    [Fact]
    public async Task The_first_admin_invite_makes_an_administrator_and_is_not_offered_again()
    {
        var token = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });
        Assert.NotNull(token);
        Assert.True((await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token }))!.GrantsAdmin);

        Assert.True((await RedeemAsync(token, "owner@example.com")).Succeeded);

        var owner = Assert.Single(await Dispatcher.QueryAsync(new GetUsersQuery()));
        Assert.Equal("owner@example.com", owner.Email);
        Assert.Equal(("Test", "User"), (owner.FirstName, owner.LastName));
        Assert.True(owner.IsAdmin);
        Assert.Null(await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) }));
    }

    [Fact]
    public async Task Reissuing_the_first_admin_invite_stops_the_previous_link()
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
        await RedeemAsync(token, "alice@example.com");

        var alice = Assert.Single(await Dispatcher.QueryAsync(new GetUsersQuery()));
        Assert.False(alice.IsAdmin);
    }

    private async Task<string> CreateInviteAsync(TimeSpan? validFor = null) =>
        (await Dispatcher.SendAsync(new CreateInviteCommand { Note = "test", ValidFor = validFor ?? TimeSpan.FromDays(7) }))
        .Token;

    private Task<RedeemInviteResultDto> RedeemAsync(string token, string email) =>
        Dispatcher.SendAsync(new RedeemInviteCommand
        {
            Token = token, FirstName = "Test", LastName = "User", Email = email, Password = Password
        });

    private async Task<AppUser?> FindUserAsync(string email)
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>().FindByEmailAsync(email);
    }

    private sealed class FakeUserContext : IUserContext
    {
        public required string UserId { get; init; }
        public bool IsAdmin { get; set; }
    }

    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
