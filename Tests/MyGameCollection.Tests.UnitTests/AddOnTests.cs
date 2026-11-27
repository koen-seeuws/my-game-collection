using MyGameCollection.Application.Abstractions.Commands.AddAddOn;
using MyGameCollection.Application.Validation.Validators.AddAddOn;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class AddOnTests
{
    private static readonly DateOnly Today = new(2026, 11, 27);

    [Fact]
    public void AddAddOn_AddsItToItsGameOnly()
    {
        var witcher = Game("The Witcher 3");
        var other = Game("Super Mario World");
        var collection = WithGames(witcher, other);

        var key = collection.AddAddOn(witcher.Key, " Hearts of Stone ", AddOnType.Expansion, GameFormat.Digital,
            purchasePrice: 9.99m, catalogId: 12503);

        var addOn = collection.Games[0].AddOns.Should().ContainSingle().Subject;
        addOn.Key.Should().Be(key);
        addOn.Title.Should().Be("Hearts of Stone");
        addOn.Type.Should().Be(AddOnType.Expansion);
        addOn.PurchasePrice.Should().Be(9.99m);
        addOn.CatalogId.Should().Be(12503);
        collection.Games[1].Should().BeSameAs(other);
    }

    [Fact]
    public void AddAddOn_UnknownGame_ThrowsKeyNotFoundException()
    {
        var collection = WithGames(Game());

        collection.Invoking(c => c.AddAddOn(Guid.NewGuid(), "Some DLC")).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AddAddOn_WithoutTitle_ThrowsAndAddsNothing()
    {
        var game = Game();
        var collection = WithGames(game);

        collection.Invoking(c => c.AddAddOn(game.Key, " ")).Should().Throw<ArgumentException>();
        collection.Games[0].AddOns.Should().BeEmpty();
    }

    [Fact]
    public void AddAddOn_DigitalOrWishlist_DropsPurchaseOrigin()
    {
        var game = Game();
        var collection = WithGames(game);

        collection.AddAddOn(game.Key, "Download", format: GameFormat.Digital, purchaseOrigin: PurchaseOrigin.Secondhand);
        collection.AddAddOn(game.Key, "Wished", format: GameFormat.Physical, status: CollectionStatus.Wishlist,
            purchaseOrigin: PurchaseOrigin.Secondhand);
        collection.AddAddOn(game.Key, "Box", format: GameFormat.Physical, purchaseOrigin: PurchaseOrigin.Secondhand);

        collection.Games[0].AddOns.Select(a => a.PurchaseOrigin).Should().Equal(null, null, PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void UpdateAddOn_ChangesItAndKeepsItsKeyAndPosition()
    {
        var collection = WithGames(Game());
        var gameKey = collection.Games[0].Key;
        var first = collection.AddAddOn(gameKey, "First");
        var second = collection.AddAddOn(gameKey, "Second");

        collection.UpdateAddOn(second, "Season Pass", AddOnType.SeasonPass);

        collection.Games[0].AddOns.Select(a => (a.Key, a.Title, a.Type))
            .Should().Equal((first, "First", AddOnType.Dlc), (second, "Season Pass", AddOnType.SeasonPass));
    }

    [Fact]
    public void UpdateGame_KeepsItsAddOns()
    {
        var collection = WithGames(Game());
        var gameKey = collection.Games[0].Key;
        collection.AddAddOn(gameKey, "Hearts of Stone");

        collection.UpdateGame(gameKey, "The Witcher 3: Wild Hunt", "PC");

        collection.Games[0].AddOns.Should().ContainSingle().Which.Title.Should().Be("Hearts of Stone");
    }

    [Fact]
    public void AcquireAddOn_WishlistAddOn_BecomesOwned()
    {
        var collection = WithGames(Game());
        var key = collection.AddAddOn(collection.Games[0].Key, "Blood and Wine", status: CollectionStatus.Wishlist);

        collection.AcquireAddOn(key, Today, 19.99m);

        var addOn = collection.Games[0].AddOns.Should().ContainSingle().Subject;
        addOn.Status.Should().Be(CollectionStatus.Owned);
        addOn.PurchaseDate.Should().Be(Today);
        addOn.PurchasePrice.Should().Be(19.99m);
    }

    [Fact]
    public void AcquireAddOn_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var collection = WithGames(Game());
        var key = collection.AddAddOn(collection.Games[0].Key, "Blood and Wine");

        collection.Invoking(c => c.AcquireAddOn(key, Today, null)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DeleteAddOn_RemovesOnlyThatAddOn()
    {
        var collection = WithGames(Game(), Game("Other"));
        var gameKey = collection.Games[0].Key;
        var kept = collection.AddAddOn(gameKey, "Kept");
        var deleted = collection.AddAddOn(gameKey, "Deleted");

        collection.DeleteAddOn(deleted);

        collection.Games[0].AddOns.Should().ContainSingle().Which.Key.Should().Be(kept);
        collection.Invoking(c => c.DeleteAddOn(deleted)).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void DeleteGame_TakesItsAddOnsAlong()
    {
        var collection = WithGames(Game());
        var gameKey = collection.Games[0].Key;
        var addOn = collection.AddAddOn(gameKey, "Hearts of Stone");

        collection.DeleteGame(gameKey);

        collection.Invoking(c => c.UpdateAddOn(addOn, "Hearts of Stone")).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AddAddOnCommand_WithoutGameOrTitle_IsInvalid()
    {
        var validator = new AddAddOnCommandInputValidator();

        validator.Validate(new AddAddOnCommand { GameKey = Guid.NewGuid(), Title = "Hearts of Stone" }).IsValid.Should().BeTrue();
        validator.Validate(new AddAddOnCommand { GameKey = Guid.Empty, Title = "Hearts of Stone" }).IsValid.Should().BeFalse();
        validator.Validate(new AddAddOnCommand { GameKey = Guid.NewGuid(), Title = "" }).IsValid.Should().BeFalse();
        validator.Validate(new AddAddOnCommand { GameKey = Guid.NewGuid(), Title = "X", Type = (AddOnType)99 }).IsValid.Should().BeFalse();
    }
}
