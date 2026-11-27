using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>The rules of the <see cref="Invite"/> aggregate, without any storage.</summary>
public class InviteOperationsTests
{
    private static readonly DateTime Now = new(2026, 11, 27, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_new_invite_is_open_until_it_expires()
    {
        var (invite, _) = InviteOperations.Create("admin-id", " For Jan ", TimeSpan.FromDays(7), Now);

        Assert.Equal("For Jan", invite.Note);
        Assert.False(invite.GrantsAdmin);
        Assert.Equal(InviteStatus.Open, invite.StatusAt(Now.AddDays(7).AddTicks(-1)));
        Assert.Equal(InviteStatus.Expired, invite.StatusAt(Now.AddDays(7)));
    }

    [Fact]
    public void The_token_holds_the_key_and_a_secret_of_which_only_the_hash_is_kept()
    {
        var (invite, token) = InviteOperations.Create("admin-id", null, TimeSpan.FromDays(7), Now);

        Assert.True(InviteToken.TryParse(token, out var key, out var secret));
        Assert.Equal(invite.AggregateKey, key);
        Assert.DoesNotContain(secret, invite.SecretHash);
        Assert.True(invite.CanBeRedeemedWith(secret, Now));
        Assert.False(invite.CanBeRedeemedWith(secret + "x", Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no-separator")]
    [InlineData("not-a-guid.secret")]
    [InlineData("0123456789abcdef0123456789abcdef.")]
    public void Malformed_tokens_are_rejected(string? token)
    {
        Assert.False(InviteToken.TryParse(token, out _, out _));
    }

    [Fact]
    public void Redeeming_uses_up_the_invite()
    {
        var (invite, secret) = NewInvite();

        invite.Redeem(secret, "alice@example.com", Now);

        Assert.Equal(InviteStatus.Used, invite.StatusAt(Now));
        Assert.Equal("alice@example.com", invite.RedeemedBy);
        Assert.Throws<InvalidOperationException>(() => invite.Redeem(secret, "bob@example.com", Now));
    }

    [Fact]
    public void An_invite_cannot_be_redeemed_with_the_wrong_secret_or_after_it_expired()
    {
        var (invite, secret) = NewInvite();

        Assert.Throws<InvalidOperationException>(() => invite.Redeem("wrong", "alice@example.com", Now));
        Assert.Throws<InvalidOperationException>(() => invite.Redeem(secret, "alice@example.com", Now.AddDays(8)));
        Assert.Equal(InviteStatus.Open, invite.StatusAt(Now));
    }

    [Fact]
    public void Only_an_open_invite_can_be_revoked()
    {
        var (invite, secret) = NewInvite();

        invite.Revoke(Now);

        Assert.Equal(InviteStatus.Revoked, invite.StatusAt(Now));
        Assert.Throws<InvalidOperationException>(() => invite.Revoke(Now));
        Assert.Throws<InvalidOperationException>(() => invite.Redeem(secret, "alice@example.com", Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(91)]
    public void An_invite_is_valid_for_up_to_90_days(int days)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InviteOperations.Create("admin-id", null, TimeSpan.FromDays(days), Now));
    }

    [Fact]
    public void A_note_is_limited_in_length()
    {
        Assert.Throws<ArgumentException>(() =>
            InviteOperations.Create("admin-id", new string('x', Invite.MaxNoteLength + 1), TimeSpan.FromDays(1), Now));
    }

    [Fact]
    public void Reissuing_the_first_admin_invite_replaces_its_secret_and_reopens_it()
    {
        var (invite, firstToken) = InviteOperations.CreateFirstAdmin(TimeSpan.FromDays(1), Now);
        InviteToken.TryParse(firstToken, out _, out var firstSecret);
        invite.Redeem(firstSecret!, "old-admin@example.com", Now);

        var secondToken = invite.ReissueFirstAdmin(TimeSpan.FromDays(1), Now.AddDays(3));
        InviteToken.TryParse(secondToken, out var key, out var secondSecret);

        Assert.Equal(Invite.FirstAdminKey, key);
        Assert.True(invite.GrantsAdmin);
        Assert.Equal(InviteStatus.Open, invite.StatusAt(Now.AddDays(3)));
        Assert.False(invite.CanBeRedeemedWith(firstSecret!, Now.AddDays(3)));
        Assert.True(invite.CanBeRedeemedWith(secondSecret!, Now.AddDays(3)));
    }

    [Fact]
    public void Only_the_first_admin_invite_can_be_reissued()
    {
        var (invite, _) = NewInvite();

        Assert.Throws<InvalidOperationException>(() => invite.ReissueFirstAdmin(TimeSpan.FromDays(1), Now));
    }

    private static (Invite Invite, string Secret) NewInvite()
    {
        var (invite, token) = InviteOperations.Create("admin-id", null, TimeSpan.FromDays(7), Now);
        InviteToken.TryParse(token, out _, out var secret);
        return (invite, secret!);
    }
}
