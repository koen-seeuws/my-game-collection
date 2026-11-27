using MyGameCollection.Application.Abstractions.Commands.AddAccessory;
using MyGameCollection.Application.Validation.Validators.AddAccessory;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class AccessoryTests
{
    private static readonly DateOnly Today = new(2026, 11, 27);

    [Fact]
    public void AddAccessory_AddsAccessoryWithNormalisedDetailsAndReturnsItsKey()
    {
        var collection = Empty();

        var key = collection.AddAccessory(" Zapper ", AccessoryType.Peripheral, platform: " NES ", manufacturer: "  ",
            colour: " Orange ", quantity: 2);

        var accessory = collection.Accessories.Should().ContainSingle().Subject;
        accessory.Key.Should().Be(key);
        accessory.Name.Should().Be("Zapper");
        accessory.Type.Should().Be(AccessoryType.Peripheral);
        accessory.Platform.Should().Be("NES");
        accessory.Manufacturer.Should().BeNull();
        accessory.Colour.Should().Be("Orange");
        accessory.Quantity.Should().Be(2);
        collection.Games.Should().BeEmpty();
        collection.Consoles.Should().BeEmpty();
    }

    [Fact]
    public void AddAccessory_WithoutPlatform_WorksWithAnySystem()
    {
        var collection = Empty();

        collection.AddAccessory("HDMI cable", AccessoryType.CableOrPower, platform: " ");

        collection.Accessories[0].Platform.Should().BeNull();
    }

    [Fact]
    public void AddAccessory_WithoutName_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddAccessory(" ")).Should().Throw<ArgumentException>();
        collection.Accessories.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(CollectionItemLimits.MaxAccessoryQuantity + 1)]
    public void AddAccessory_QuantityOutOfRange_Throws(int quantity)
    {
        var collection = Empty();

        collection.Invoking(c => c.AddAccessory("Wii Remote", quantity: quantity))
            .Should().Throw<ArgumentOutOfRangeException>();
        collection.Accessories.Should().BeEmpty();
    }

    [Fact]
    public void AddAccessory_UnknownType_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddAccessory("Thing", (AccessoryType)99)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddAccessory_OnWishlist_DropsPurchaseOrigin()
    {
        var collection = Empty();

        collection.AddAccessory("GameCube Controller", status: CollectionStatus.Wishlist, purchaseOrigin: PurchaseOrigin.New);

        collection.Accessories[0].PurchaseOrigin.Should().BeNull();
    }

    [Fact]
    public void UpdateAccessory_ExistingKey_ReplacesDetails()
    {
        var accessory = Accessory();
        var collection = WithAccessories(accessory);

        collection.UpdateAccessory(accessory.Key, "DualShock 2", AccessoryType.Controller, platform: "PlayStation 2",
            colour: "Ocean Blue", quantity: 3);

        collection.Accessories[0].Key.Should().Be(accessory.Key);
        collection.Accessories[0].Colour.Should().Be("Ocean Blue");
        collection.Accessories[0].Quantity.Should().Be(3);
    }

    [Fact]
    public void UpdateAccessory_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = WithAccessories(Accessory());

        collection.Invoking(c => c.UpdateAccessory(Guid.NewGuid(), "Memory Card")).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void DeleteAccessory_RemovesOnlyMatchingAccessory()
    {
        var keep = Accessory("Keep");
        var remove = Accessory("Remove");
        var collection = WithAccessories(keep, remove);

        collection.DeleteAccessory(remove.Key);

        collection.Accessories.Should().Equal(keep);
    }

    [Fact]
    public void DeleteAccessory_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = Empty();

        collection.Invoking(c => c.DeleteAccessory(Guid.NewGuid())).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void AcquireAccessory_WishlistAccessory_BecomesOwnedWithPurchaseDetails()
    {
        var wished = Accessory(status: CollectionStatus.Wishlist);
        var collection = WithAccessories(wished);

        collection.AcquireAccessory(wished.Key, Today, 25m, PurchaseOrigin.Secondhand);

        var acquired = collection.Accessories.Should().ContainSingle().Subject;
        acquired.Status.Should().Be(CollectionStatus.Owned);
        acquired.PurchaseDate.Should().Be(Today);
        acquired.PurchasePrice.Should().Be(25m);
        acquired.PurchaseOrigin.Should().Be(PurchaseOrigin.Secondhand);
    }

    [Fact]
    public void AcquireAccessory_AlreadyOwned_ThrowsInvalidOperationException()
    {
        var owned = Accessory();
        var collection = WithAccessories(owned);

        collection.Invoking(c => c.AcquireAccessory(owned.Key, Today, 10m)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AcquireAccessory_NegativePrice_ThrowsAndLeavesItOnWishlist()
    {
        var wished = Accessory(status: CollectionStatus.Wishlist);
        var collection = WithAccessories(wished);

        collection.Invoking(c => c.AcquireAccessory(wished.Key, Today, -5m)).Should().Throw<ArgumentOutOfRangeException>();
        collection.Accessories[0].Status.Should().Be(CollectionStatus.Wishlist);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(CollectionItemLimits.MaxAccessoryQuantity + 1)]
    public void AddAccessoryCommand_QuantityOutOfRange_IsRejected(int quantity)
    {
        var result = new AddAccessoryCommandInputValidator().Validate(new AddAccessoryCommand
        {
            Name = "Wii Remote",
            Quantity = quantity
        });

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AddAccessoryCommand.Quantity));
    }

    [Fact]
    public void AddAccessoryCommand_WithOnlyAName_IsValid()
    {
        var result = new AddAccessoryCommandInputValidator().Validate(new AddAccessoryCommand { Name = "Memory Card" });

        result.IsValid.Should().BeTrue();
    }
}
