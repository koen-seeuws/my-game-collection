using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.IGDB;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class GameCompanyTests
{
    [Fact]
    public void AddGame_WithoutCompanies_HasNone()
    {
        var collection = Empty();

        collection.AddGame("Super Mario World", "Super Nintendo");

        collection.Games[0].Developers.Should().BeEmpty();
        collection.Games[0].Publishers.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_KeepsCompaniesTrimmedInOrder_WithoutBlanksOrDuplicates()
    {
        var collection = Empty();

        collection.AddGame("Minecraft", "PC",
            developers: [" Mojang Studios ", "", "4J Studios", "mojang studios"],
            publishers: ["Microsoft", "  "]);

        collection.Games[0].Developers.Should().Equal("Mojang Studios", "4J Studios");
        collection.Games[0].Publishers.Should().Equal("Microsoft");
    }

    [Fact]
    public void UpdateGame_ReplacesAndClearsTheCompanies()
    {
        var collection = Empty();
        var key = collection.AddGame("Goat Simulator 3", "PlayStation 5", developers: ["Coffee Stain North"]);

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5",
            developers: ["Coffee Stain North"], publishers: ["Coffee Stain Publishing"]);
        collection.Games[0].Publishers.Should().Equal("Coffee Stain Publishing");

        collection.UpdateGame(key, "Goat Simulator 3", "PlayStation 5");
        collection.Games[0].Developers.Should().BeEmpty();
        collection.Games[0].Publishers.Should().BeEmpty();
    }

    [Fact]
    public void AddGame_WithTooManyPublishers_Throws()
    {
        var collection = Empty();
        var publishers = Enumerable.Range(1, CollectionItemLimits.MaxCompanies + 1).Select(i => $"Publisher {i}");

        collection.Invoking(c => c.AddGame("Everywhere", "PC", publishers: publishers)).Should().Throw<ArgumentException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddGameCommand_WithTooManyOrTooLongCompanies_IsInvalid()
    {
        var validator = new AddGameCommandInputValidator();

        validator.Validate(new AddGameCommand { Title = "Zelda", Platform = "NES", Developers = ["Nintendo R&D4"], Publishers = ["Nintendo"] })
            .IsValid.Should().BeTrue();
        validator.Validate(new AddGameCommand
            {
                Title = "Zelda", Platform = "NES",
                Developers = Enumerable.Range(1, CollectionItemLimits.MaxCompanies + 1).Select(i => $"Studio {i}").ToList()
            })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Developers");
        validator.Validate(new AddGameCommand
            {
                Title = "Zelda", Platform = "NES",
                Publishers = [new string('x', CollectionItemLimits.MaxShortTextLength + 1)]
            })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Publishers");
    }

    [Fact]
    public void Igdb_SplitsInvolvedCompaniesIntoDevelopersAndPublishers()
    {
        var game = IgdbGame(
            new IgdbInvolvedCompany(new IgdbCompany("Nintendo EPD"), Developer: true),
            new IgdbInvolvedCompany(new IgdbCompany(" Nintendo "), Publisher: true),
            new IgdbInvolvedCompany(new IgdbCompany("Monolith Soft"), Developer: true),
            new IgdbInvolvedCompany(new IgdbCompany("Panic Button")), // only ported it
            new IgdbInvolvedCompany(new IgdbCompany("nintendo"), Publisher: true),
            new IgdbInvolvedCompany(null, Developer: true));

        IgdbCompanies.Developers(game).Should().Equal("Nintendo EPD", "Monolith Soft");
        IgdbCompanies.Publishers(game).Should().Equal("Nintendo");
    }

    [Fact]
    public void Igdb_KeepsNoMoreCompaniesThanAGameCanHold()
    {
        var game = IgdbGame(Enumerable.Range(1, CollectionItemLimits.MaxCompanies + 5)
            .Select(i => new IgdbInvolvedCompany(new IgdbCompany($"Regional publisher {i}"), Publisher: true))
            .ToArray());

        IgdbCompanies.Publishers(game).Should().HaveCount(CollectionItemLimits.MaxCompanies);
    }

    private static IgdbGame IgdbGame(params IgdbInvolvedCompany[] companies) =>
        new(Id: 1, Name: "Xenoblade Chronicles", VersionTitle: null, FirstReleaseDate: null, ReleaseDates: null, Cover: null,
            Platforms: null, Genres: null, InvolvedCompanies: companies);
}
