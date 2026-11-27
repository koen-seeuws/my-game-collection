using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class GameGenreTests
{
    [Fact]
    public void AddGame_WithoutGenres_HasNone()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo");

        collection.Games[0].Genres.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_KeepsGenresTrimmedInOrder_WithoutBlanksOrDuplicates()
    {
        var collection = Empty();

        collection.AddGame("Chrono Trigger", "Super Nintendo",
            genres: [" Role-playing (RPG) ", "Adventure", "", "  ", "role-playing (rpg)"]);

        collection.Games[0].Genres.Should().Equal("Role-playing (RPG)", "Adventure");
    }

    [Fact]
    public void UpdateGame_ReplacesAndClearsTheGenres()
    {
        var collection = Empty();
        var key = collection.AddGame("Goat Simulator 3", "PlayStation 5", genres: ["Simulator"]);

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5", genres: ["Simulator", "Adventure"]);
        collection.Games[0].Genres.Should().Equal("Simulator", "Adventure");

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5");
        collection.Games[0].Genres.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_WithTooManyGenres_Throws()
    {
        var collection = Empty();
        var genres = Enumerable.Range(1, CollectionItemLimits.MaxGenres + 1).Select(i => $"Genre {i}");

        collection.Invoking(c => c.AddGame("Everything", "PC", genres: genres)).Should().Throw<ArgumentException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_WithTooLongGenre_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Zelda", "NES", genres: [new string('x', CollectionItemLimits.MaxShortTextLength + 1)]))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGameCommand_WithTooManyOrTooLongGenres_IsInvalid()
    {
        var validator = new AddGameCommandInputValidator();

        validator.Validate(new AddGameCommand { Title = "Zelda", Platform = "NES", Genres = ["Adventure", "Role-playing (RPG)"] })
            .IsValid.Should().BeTrue();
        validator.Validate(new AddGameCommand
            {
                Title = "Zelda", Platform = "NES",
                Genres = Enumerable.Range(1, CollectionItemLimits.MaxGenres + 1).Select(i => $"Genre {i}").ToList()
            })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Genres");
        validator.Validate(new AddGameCommand
            {
                Title = "Zelda", Platform = "NES",
                Genres = [new string('x', CollectionItemLimits.MaxShortTextLength + 1)]
            })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Genres");
    }
}
