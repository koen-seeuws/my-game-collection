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

        var acquired = Assert.Single(collection.Games);
        Assert.Equal(wished.Key, acquired.Key);
        Assert.Equal("Chrono Trigger", acquired.Title);
        Assert.Equal(CollectionStatus.Owned, acquired.Status);
        Assert.Equal(Today, acquired.PurchaseDate);
        Assert.Equal(120m, acquired.PurchasePrice);
    }

    [Fact]
    public void AcquireGame_WithoutPrice_IsAllowed()
    {
        var wished = Game(status: CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        collection.AcquireGame(wished.Key, Today, null);

        Assert.Null(collection.Games[0].PurchasePrice);
    }

    [Fact]
    public void AcquireGame_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var owned = Game(status: CollectionStatus.Owned);
        var collection = WithGames(owned);

        Assert.Throws<InvalidOperationException>(() => collection.AcquireGame(owned.Key, Today, 10m));
        Assert.Same(owned, collection.Games[0]);
    }

    [Fact]
    public void AcquireGame_NegativePrice_ThrowsAndLeavesGameOnWishlist()
    {
        var wished = Game(status: CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        Assert.Throws<ArgumentOutOfRangeException>(() => collection.AcquireGame(wished.Key, Today, -5m));
        Assert.Equal(CollectionStatus.Wishlist, collection.Games[0].Status);
    }

    [Fact]
    public void AcquireGame_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = Empty();

        Assert.Throws<KeyNotFoundException>(() => collection.AcquireGame(Guid.NewGuid(), Today, null));
    }

    [Fact]
    public void AcquireConsole_WishlistConsole_BecomesOwnedWithPurchaseDetails()
    {
        var wished = Console("Dreamcast", CollectionStatus.Wishlist);
        var collection = WithConsoles(wished);

        collection.AcquireConsole(wished.Key, Today, 80m);

        var acquired = Assert.Single(collection.Consoles);
        Assert.Equal(CollectionStatus.Owned, acquired.Status);
        Assert.Equal(Today, acquired.PurchaseDate);
        Assert.Equal(80m, acquired.PurchasePrice);
    }

    [Fact]
    public void AcquireConsole_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var owned = Console();
        var collection = WithConsoles(owned);

        Assert.Throws<InvalidOperationException>(() => collection.AcquireConsole(owned.Key, Today, null));
    }
}
