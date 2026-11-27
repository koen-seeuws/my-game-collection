using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class AcquireTests
{
    private static readonly DateOnly Today = new(2026, 11, 26);

    [Fact]
    public void AcquireGame_WishlistGame_BecomesOwnedWithPurchaseDetails()
    {
        var wished = Game("Chrono Trigger", CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        collection.AcquireGame(wished.Key, Today, 120m);

        var acquired = collection.Games.Should().ContainSingle().Subject;
        acquired.Key.Should().Be(wished.Key);
        acquired.Title.Should().Be("Chrono Trigger");
        acquired.Status.Should().Be(CollectionStatus.Owned);
        acquired.PurchaseDate.Should().Be(Today);
        acquired.PurchasePrice.Should().Be(120m);
    }

    [Fact]
    public void AcquireGame_WithoutPrice_IsAllowed()
    {
        var wished = Game(status: CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        collection.AcquireGame(wished.Key, Today, null);

        collection.Games[0].PurchasePrice.Should().BeNull();
    }

    [Fact]
    public void AcquireGame_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var owned = Game(status: CollectionStatus.Owned);
        var collection = WithGames(owned);

        collection.Invoking(c => c.AcquireGame(owned.Key, Today, 10m)).Should().Throw<InvalidOperationException>();
        collection.Games[0].Should().BeSameAs(owned);
    }

    [Fact]
    public void AcquireGame_NegativePrice_ThrowsAndLeavesGameOnWishlist()
    {
        var wished = Game(status: CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        collection.Invoking(c => c.AcquireGame(wished.Key, Today, -5m)).Should().Throw<ArgumentOutOfRangeException>();
        collection.Games[0].Status.Should().Be(CollectionStatus.Wishlist);
    }

    [Fact]
    public void AcquireGame_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = Empty();

        collection.Invoking(c => c.AcquireGame(Guid.NewGuid(), Today, null)).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AcquireConsole_WishlistConsole_BecomesOwnedWithPurchaseDetails()
    {
        var wished = Console("Dreamcast", CollectionStatus.Wishlist);
        var collection = WithConsoles(wished);

        collection.AcquireConsole(wished.Key, Today, 80m);

        var acquired = collection.Consoles.Should().ContainSingle().Subject;
        acquired.Status.Should().Be(CollectionStatus.Owned);
        acquired.PurchaseDate.Should().Be(Today);
        acquired.PurchasePrice.Should().Be(80m);
    }

    [Fact]
    public void AcquireConsole_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var owned = Console();
        var collection = WithConsoles(owned);

        collection.Invoking(c => c.AcquireConsole(owned.Key, Today, null)).Should().Throw<InvalidOperationException>();
    }
}
