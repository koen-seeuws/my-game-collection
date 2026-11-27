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

        Assert.Null(Assert.Single(collection.Games).Edition);
    }

    [Fact]
    public void AddGame_WithEdition_StoresItTrimmed()
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", edition: "  Pre-Udder Edition ");

        Assert.Equal("Pre-Udder Edition", Assert.Single(collection.Games).Edition);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddGame_WithBlankEdition_StoresNoEdition(string edition)
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", edition: edition);

        Assert.Null(Assert.Single(collection.Games).Edition);
    }

    [Fact]
    public void AddGame_WithTooLongEdition_Throws()
    {
        var collection = Empty();

        var ex = Assert.Throws<ArgumentException>(() =>
            collection.AddGame("Goat Simulator 3", "PlayStation 5", edition: new string('x', 101)));

        Assert.Equal("edition", ex.ParamName);
        Assert.Empty(collection.Games);
    }

    [Fact]
    public void UpdateGame_ChangesAndClearsTheEdition()
    {
        var collection = Empty();
        var key = collection.AddGame("Goat Simulator 3", "PlayStation 5");

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5", edition: "Goat in a Box Edition");
        Assert.Equal("Goat in a Box Edition", collection.Games[0].Edition);

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5");
        Assert.Null(collection.Games[0].Edition);
    }
}
