using MyGameCollection.Core.Domain;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class GameEditionTests
{
    [Fact]
    public void AddGame_WithoutEdition_IsTheStandardRelease()
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5");

        collection.Games.Should().ContainSingle().Which.Edition.Should().BeNull();
    }

    [Fact]
    public void AddGame_WithEdition_StoresItTrimmed()
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", edition: "  Pre-Udder Edition ");

        collection.Games.Should().ContainSingle().Which.Edition.Should().Be("Pre-Udder Edition");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddGame_WithBlankEdition_StoresNoEdition(string edition)
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", edition: edition);

        collection.Games.Should().ContainSingle().Which.Edition.Should().BeNull();
    }

    [Fact]
    public void AddGame_WithTooLongEdition_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Goat Simulator 3", "PlayStation 5", edition: new string('x', 101)))
            .Should().Throw<ArgumentException>()
            .WithParameterName("edition");
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void UpdateGame_ChangesAndClearsTheEdition()
    {
        var collection = Empty();
        var key = collection.AddGame("Goat Simulator 3", "PlayStation 5");

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5", edition: "Goat in a Box Edition");
        collection.Games[0].Edition.Should().Be("Goat in a Box Edition");

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5");
        collection.Games[0].Edition.Should().BeNull();
    }
}
