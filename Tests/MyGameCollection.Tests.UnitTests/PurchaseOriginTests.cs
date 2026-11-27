using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Validation.Validators.AddConsole;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class PurchaseOriginTests
{
    private static readonly DateOnly Today = new(2026, 11, 27);

    [Fact]
    public void AddGame_WithoutOrigin_IsUnknown()
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo");

        collection.Games.Should().ContainSingle().Which.PurchaseOrigin.Should().BeNull();
    }

    [Theory]
    [InlineData(PurchaseOrigin.New)]
    [InlineData(PurchaseOrigin.Secondhand)]
    public void AddGame_PhysicalOwnedWithOrigin_StoresIt(PurchaseOrigin origin)
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", purchaseOrigin: origin);

        collection.Games[0].PurchaseOrigin.Should().Be(origin);
    }

    [Theory]
    [InlineData(GameFormat.Digital)]
    [InlineData(GameFormat.Subscription)]
    public void AddGame_NotPhysical_HasNoOrigin(GameFormat format)
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", format: format, purchaseOrigin: PurchaseOrigin.Secondhand);

        collection.Games[0].PurchaseOrigin.Should().BeNull();
    }

    [Fact]
    public void AddGame_OnWishlist_HasNoOrigin()
    {
        var collection = Empty();

        collection.AddGame("Super Metroid", "Super Nintendo", status: CollectionStatus.Wishlist,
            purchaseOrigin: PurchaseOrigin.New);

        collection.Games[0].PurchaseOrigin.Should().BeNull();
    }

    [Fact]
    public void UpdateGame_ChangesTheOrigin()
    {
        var collection = Empty();
        var key = collection.AddGame("Super Metroid", "Super Nintendo", purchaseOrigin: PurchaseOrigin.New);

        collection.UpdateGame(key, "Super Metroid", "Super Nintendo", purchaseOrigin: PurchaseOrigin.Secondhand);

        collection.Games[0].PurchaseOrigin.Should().Be(PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void AddConsole_WithOrigin_StoresIt()
    {
        var collection = Empty();

        collection.AddConsole("Super Nintendo", "Nintendo", purchaseOrigin: PurchaseOrigin.Secondhand);

        collection.Consoles.Should().ContainSingle().Which.PurchaseOrigin.Should().Be(PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void UpdateConsole_OnWishlist_DropsTheOrigin()
    {
        var collection = Empty();
        var key = collection.AddConsole("Super Nintendo", "Nintendo", purchaseOrigin: PurchaseOrigin.New);

        collection.UpdateConsole(key, "Super Nintendo", "Nintendo", status: CollectionStatus.Wishlist,
            purchaseOrigin: PurchaseOrigin.New);

        collection.Consoles[0].PurchaseOrigin.Should().BeNull();
    }

    [Fact]
    public void AcquireGame_WithOrigin_StoresIt()
    {
        var wished = Game("Chrono Trigger", CollectionStatus.Wishlist);
        var collection = WithGames(wished);

        collection.AcquireGame(wished.Key, Today, 120m, PurchaseOrigin.Secondhand);

        collection.Games[0].PurchaseOrigin.Should().Be(PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void AcquireGame_Digital_HasNoOrigin()
    {
        var wished = Game("Chrono Trigger", CollectionStatus.Wishlist) with { Format = GameFormat.Digital };
        var collection = WithGames(wished);

        collection.AcquireGame(wished.Key, Today, 9.99m, PurchaseOrigin.New);

        collection.Games[0].PurchaseOrigin.Should().BeNull();
    }

    [Fact]
    public void AcquireConsole_WithOrigin_StoresIt()
    {
        var wished = Console("Neo Geo AES", CollectionStatus.Wishlist);
        var collection = WithConsoles(wished);

        collection.AcquireConsole(wished.Key, Today, 450m, PurchaseOrigin.Secondhand);

        collection.Consoles[0].PurchaseOrigin.Should().Be(PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void AddConsoleCommand_WithUnknownOrigin_IsInvalid()
    {
        var result = new AddConsoleCommandInputValidator().Validate(new AddConsoleCommand
        {
            Name = "Super Nintendo",
            Manufacturer = "Nintendo",
            PurchaseOrigin = (PurchaseOrigin)42
        });

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AddConsoleCommand.PurchaseOrigin));
    }
}
