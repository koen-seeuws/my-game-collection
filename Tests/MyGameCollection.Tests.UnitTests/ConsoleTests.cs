using MyGameCollection.Core.Domain;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class ConsoleTests
{
    [Fact]
    public void AddConsole_AddsConsoleWithNormalisedDetailsAndReturnsItsKey()
    {
        var collection = Empty();

        var key = collection.AddConsole(" PlayStation ", " Sony ", model: "  ", colour: " Tribal Edition ");

        var console = collection.Consoles.Should().ContainSingle().Subject;
        console.Key.Should().Be(key);
        console.Name.Should().Be("PlayStation");
        console.Manufacturer.Should().Be("Sony");
        console.Model.Should().BeNull();
        console.Colour.Should().Be("Tribal Edition");
        collection.Games.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Sony")]
    [InlineData("PlayStation", " ")]
    public void AddConsole_WithoutNameOrManufacturer_Throws(string name, string manufacturer)
    {
        var collection = Empty();

        collection.Invoking(c => c.AddConsole(name, manufacturer)).Should().Throw<ArgumentException>();
        collection.Consoles.Should().BeEmpty();
    }

    [Fact]
    public void UpdateConsole_ExistingKey_ReplacesDetails()
    {
        var console = Console("Mega Drive");
        var collection = WithConsoles(console);

        collection.UpdateConsole(console.Key, "Mega Drive", "Sega", model: "Model 2", colour: "Pearl Pink");

        collection.Consoles[0].Key.Should().Be(console.Key);
        collection.Consoles[0].Model.Should().Be("Model 2");
        collection.Consoles[0].Colour.Should().Be("Pearl Pink");
    }

    [Fact]
    public void UpdateConsole_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = WithConsoles(Console());

        collection.Invoking(c => c.UpdateConsole(Guid.NewGuid(), "Super Nintendo", "Nintendo"))
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void DeleteConsole_RemovesOnlyMatchingConsole()
    {
        var keep = Console("Keep");
        var remove = Console("Remove");
        var collection = WithConsoles(keep, remove);

        collection.DeleteConsole(remove.Key);

        collection.Consoles.Should().Equal(keep);
    }

    [Fact]
    public void DeleteConsole_UnknownKey_ThrowsKeyNotFoundException()
    {
        var collection = Empty();

        collection.Invoking(c => c.DeleteConsole(Guid.NewGuid())).Should().Throw<KeyNotFoundException>();
    }
}
