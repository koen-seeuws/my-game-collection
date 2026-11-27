using MyGameCollection.Core.Domain;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class ConsoleTests
{
    [Fact]
    public void AddConsole_AddsConsoleWithNormalisedDetailsAndReturnsItsKey()
    {
        var collection = Empty();

        var key = collection.AddConsole(" PlayStation ", " Sony ", model: "  ");

        var console = Assert.Single(collection.Consoles);
        Assert.Equal(key, console.Key);
        Assert.Equal("PlayStation", console.Name);
        Assert.Equal("Sony", console.Manufacturer);
        Assert.Null(console.Model);
        Assert.Empty(collection.Games);
    }

    [Theory]
    [InlineData("", "Sony")]
    [InlineData("PlayStation", " ")]
    public void AddConsole_WithoutNameOrManufacturer_Throws(string name, string manufacturer)
    {
        var collection = Empty();

        Assert.Throws<ArgumentException>(() => collection.AddConsole(name, manufacturer));
        Assert.Empty(collection.Consoles);
    }

    [Fact]
    public void UpdateConsole_ExistingKey_ReplacesDetails()
    {
        var console = Console("Mega Drive");
        var collection = WithConsoles(console);

        collection.UpdateConsole(console.Key, "Mega Drive", "Sega", model: "Model 2");

        Assert.Equal(console.Key, collection.Consoles[0].Key);
        Assert.Equal("Model 2", collection.Consoles[0].Model);
    }

    [Fact]
    public void UpdateConsole_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = WithConsoles(Console());

        Assert.Throws<KeyNotFoundException>(() => collection.UpdateConsole(Guid.NewGuid(), "Super Nintendo", "Nintendo"));
    }

    [Fact]
    public void DeleteConsole_RemovesOnlyMatchingConsole()
    {
        var keep = Console("Keep");
        var remove = Console("Remove");
        var collection = WithConsoles(keep, remove);

        collection.DeleteConsole(remove.Key);

        Assert.Equal([keep], collection.Consoles);
    }

    [Fact]
    public void DeleteConsole_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = Empty();

        Assert.Throws<KeyNotFoundException>(() => collection.DeleteConsole(Guid.NewGuid()));
    }
}
