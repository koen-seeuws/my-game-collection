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

        first.Succeeded.Should().BeTrue();
        second.Succeeded.Should().BeFalse();
        (await FindUserAsync("bob@example.com")).Should().BeNull();

        var invite = (await Dispatcher.QueryAsync(new GetInvitesQuery())).Should().ContainSingle().Subject;
        invite.Status.Should().Be(InviteStatus.Used);
        invite.RedeemedBy.Should().Be("alice@example.com");
        invite.CreatedBy.Should().Be("admin-id");
    }

    [Fact]
    public async Task An_expired_invite_cannot_be_used()
    {
        var token = await CreateInviteAsync(TimeSpan.FromDays(1));
        _time.Advance(TimeSpan.FromDays(1) + TimeSpan.FromSeconds(1));

        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token })).Should().BeNull();
        (await RedeemAsync(token, "alice@example.com")).Succeeded.Should().BeFalse();
        (await Dispatcher.QueryAsync(new GetInvitesQuery())).Should().ContainSingle()
            .Which.Status.Should().Be(InviteStatus.Expired);
    }

    [Fact]
    public async Task A_revoked_invite_cannot_be_used_or_revoked_again()
    {
        var token = await CreateInviteAsync();
        var invite = (await Dispatcher.QueryAsync(new GetInvitesQuery())).Should().ContainSingle().Subject;

        await Dispatcher.SendAsync(new RevokeInviteCommand { Id = invite.Id });

        (await RedeemAsync(token, "alice@example.com")).Succeeded.Should().BeFalse();
        await Dispatcher.Awaiting(d => d.SendAsync(new RevokeInviteCommand { Id = invite.Id }))
            .Should().ThrowAsync<InvalidOperationException>();
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

        tooShort.Succeeded.Should().BeFalse();
        tooShort.Errors.Should().NotBeEmpty();
        retried.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        await CreateInviteAsync();

        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = "not-a-real-token" })).Should().BeNull();
        (await RedeemAsync("not-a-real-token", "alice@example.com")).Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Only_administrators_can_manage_invites_and_see_users()
    {
        var token = await CreateInviteAsync();
        var invite = (await Dispatcher.QueryAsync(new GetInvitesQuery())).Should().ContainSingle().Subject;
        _user.IsAdmin = false;

        await this.Awaiting(t => t.CreateInviteAsync(null)).Should().ThrowAsync<UnauthorizedAccessException>();
        await Dispatcher.Awaiting(d => d.SendAsync(new RevokeInviteCommand { Id = invite.Id }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
        await Dispatcher.Awaiting(d => d.QueryAsync(new GetInvitesQuery())).Should().ThrowAsync<UnauthorizedAccessException>();
        await Dispatcher.Awaiting(d => d.QueryAsync(new GetUsersQuery())).Should().ThrowAsync<UnauthorizedAccessException>();

        // Opening and using a link needs no signed-in user at all.
        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token })).Should().NotBeNull();
        (await RedeemAsync(token, "alice@example.com")).Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task An_email_address_can_only_have_one_account()
    {
        await RedeemAsync(await CreateInviteAsync(), "alice@example.com");

        var duplicate = await RedeemAsync(await CreateInviteAsync(), "ALICE@example.com");

        duplicate.Succeeded.Should().BeFalse();
        duplicate.Errors.Should().ContainSingle().Which.Should().Contain("taken");
    }

    [Fact]
    public async Task Names_are_required()
    {
        var token = await CreateInviteAsync();

        var rejected = await Dispatcher.Awaiting(d => d.SendAsync(new RedeemInviteCommand
            {
                Token = token, FirstName = " ", LastName = "User", Email = "a@example.com", Password = Password
            }))
            .Should().ThrowAsync<CommandValidationException>();

        rejected.Which.Errors.Should().ContainSingle().Which.Should().Be("First Name is required.");
        (await FindUserAsync("a@example.com")).Should().BeNull();
    }

    [Fact]
    public async Task Redeeming_replies_with_the_new_account_id()
    {
        var result = await RedeemAsync(await CreateInviteAsync(), "alice@example.com");

        result.UserId.Should().Be((await FindUserAsync("alice@example.com"))!.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public async Task An_invite_must_be_valid_for_up_to_90_days(int days)
    {
        await this.Awaiting(t => t.CreateInviteAsync(TimeSpan.FromDays(days))).Should().ThrowAsync<CommandValidationException>();
    }

    [Fact]
    public async Task The_first_admin_invite_makes_an_administrator_and_is_not_offered_again()
    {
        var token = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });
        token.Should().NotBeNull();
        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = token! }))!.GrantsAdmin.Should().BeTrue();

        (await RedeemAsync(token!, "owner@example.com")).Succeeded.Should().BeTrue();

        var owner = (await Dispatcher.QueryAsync(new GetUsersQuery())).Should().ContainSingle().Subject;
        owner.Email.Should().Be("owner@example.com");
        (owner.FirstName, owner.LastName).Should().Be(("Test", "User"));
        owner.IsAdmin.Should().BeTrue();
        (await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) })).Should().BeNull();
    }

    [Fact]
    public async Task A_new_first_admin_invite_revokes_the_previous_one()
    {
        var first = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });
        var second = await Dispatcher.SendAsync(new CreateFirstAdminInviteCommand { ValidFor = TimeSpan.FromDays(1) });

        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = first! })).Should().BeNull();
        (await Dispatcher.QueryAsync(new GetOpenInviteQuery { Token = second! })).Should().NotBeNull();
    }

    [Fact]
    public async Task A_normal_invite_does_not_make_an_administrator()
    {
        var token = await CreateInviteAsync();
        await RedeemAsync(token, "alice@example.com");

        (await Dispatcher.QueryAsync(new GetUsersQuery())).Should().ContainSingle()
            .Which.IsAdmin.Should().BeFalse();
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
