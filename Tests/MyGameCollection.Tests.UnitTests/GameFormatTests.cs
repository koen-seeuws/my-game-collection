using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class GameFormatTests
{
    [Fact]
    public void AddGame_WithoutFormat_IsPhysical()
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5");

        collection.Games.Should().ContainSingle().Which.Format.Should().Be(GameFormat.Physical);
    }

    [Theory]
    [InlineData(GameFormat.Digital)]
    [InlineData(GameFormat.Subscription)]
    public void AddGame_WithFormat_StoresIt(GameFormat format)
    {
        var collection = Empty();

        collection.AddGame("Goat Simulator 3", "PlayStation 5", format: format);

        collection.Games.Should().ContainSingle().Which.Format.Should().Be(format);
    }

    [Fact]
    public void UpdateGame_ChangesTheFormat()
    {
        var collection = Empty();
        var key = collection.AddGame("Goat Simulator 3", "PlayStation 5", format: GameFormat.Subscription);

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5", format: GameFormat.Digital);

        collection.Games[0].Format.Should().Be(GameFormat.Digital);
    }

    [Fact]
    public void AddGameCommand_WithUnknownFormat_IsInvalid()
    {
        var result = new AddGameCommandInputValidator().Validate(new AddGameCommand
        {
            Title = "Goat Simulator 3",
            Platform = "PlayStation 5",
            Format = (GameFormat)42
        });

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(AddGameCommand.Format));
    }
}
