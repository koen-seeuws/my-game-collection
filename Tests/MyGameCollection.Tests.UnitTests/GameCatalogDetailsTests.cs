using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.IGDB;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>The catalog id, series, type and age rating of a game, and how they are picked from IGDB.</summary>
public class GameCatalogDetailsTests
{
    [Fact]
    public void AddGame_KeepsCatalogDetailsTrimmed()
    {
        var collection = Empty();

        collection.AddGame("Super Mario Odyssey", "Nintendo Switch",
            series: " Super Mario ", type: GameType.MainGame, ageRating: " PEGI 7 ", catalogId: 26758);

        var game = collection.Games[0];
        game.Series.Should().Be("Super Mario");
        game.Type.Should().Be(GameType.MainGame);
        game.AgeRating.Should().Be("PEGI 7");
        game.CatalogId.Should().Be(26758);
    }

    [Fact]
    public void AddGame_WithBlankSeriesAndRating_LeavesThemEmpty()
    {
        var collection = Empty();

        collection.AddGame("Tetris", "Game Boy", series: "  ", ageRating: "");

        collection.Games[0].Series.Should().BeNull();
        collection.Games[0].AgeRating.Should().BeNull();
    }

    [Fact]
    public void UpdateGame_ReplacesAndClearsCatalogDetails()
    {
        var collection = Empty();
        var key = collection.AddGame("Super Mario 64", "Nintendo 64", series: "Super Mario", catalogId: 1074);

        collection.UpdateGame(key, "Super Mario 64", "Nintendo 64");

        collection.Games[0].Series.Should().BeNull();
        collection.Games[0].CatalogId.Should().BeNull();
    }

    [Fact]
    public void AddGame_WithInvalidCatalogIdOrType_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Zelda", "NES", catalogId: 0)).Should().Throw<ArgumentOutOfRangeException>();
        collection.Invoking(c => c.AddGame("Zelda", "NES", type: (GameType)99)).Should().Throw<ArgumentOutOfRangeException>();
        collection.Games.Should().BeEmpty();
    }

    [Fact]
    public void AddConsole_KeepsCatalogId()
    {
        var collection = Empty();

        collection.AddConsole("Nintendo Switch", "Nintendo", catalogId: 130);

        collection.Consoles[0].CatalogId.Should().Be(130);
    }

    [Fact]
    public void AddGameCommand_WithInvalidCatalogDetails_IsInvalid()
    {
        var validator = new AddGameCommandInputValidator();

        validator.Validate(new AddGameCommand { Title = "Zelda", Platform = "NES", CatalogId = 1025, Type = GameType.MainGame })
            .IsValid.Should().BeTrue();
        validator.Validate(new AddGameCommand { Title = "Zelda", Platform = "NES", CatalogId = -1 })
            .Errors.Should().ContainSingle(e => e.PropertyName == "CatalogId");
        validator.Validate(new AddGameCommand { Title = "Zelda", Platform = "NES", Type = (GameType)99 })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Type");
        validator.Validate(new AddGameCommand
            {
                Title = "Zelda", Platform = "NES", Series = new string('x', CollectionItemLimits.MaxShortTextLength + 1)
            })
            .Errors.Should().ContainSingle(e => e.PropertyName == "Series");
    }

    [Fact]
    public void Igdb_SeriesPrefersCollectionOverFranchise()
    {
        IgdbGameDetails.Series(IgdbGame() with
            {
                Collections = [new IgdbNamed(" Super Mario "), new IgdbNamed("Super Mario 64")],
                Franchise = new IgdbNamed("Mario")
            })
            .Should().Be("Super Mario");
        IgdbGameDetails.Series(IgdbGame() with { Franchise = new IgdbNamed("Mario") }).Should().Be("Mario");
        IgdbGameDetails.Series(IgdbGame() with { Franchises = [new IgdbNamed("Mario")] }).Should().Be("Mario");
        IgdbGameDetails.Series(IgdbGame()).Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0L, GameType.MainGame)]
    [InlineData(1L, GameType.Dlc)]
    [InlineData(13L, GameType.Dlc)]
    [InlineData(4L, GameType.Expansion)]
    [InlineData(3L, GameType.Bundle)]
    [InlineData(8L, GameType.Remake)]
    [InlineData(9L, GameType.Remaster)]
    [InlineData(10L, GameType.ExpandedGame)]
    [InlineData(11L, GameType.Port)]
    [InlineData(5L, GameType.Other)]
    public void Igdb_MapsGameTypes(long? igdbType, GameType? expected) =>
        IgdbGameDetails.Type(IgdbGame() with { GameType = igdbType }).Should().Be(expected);

    [Fact]
    public void Igdb_KeepsCompleteAgeRatings_OnePerBoard()
    {
        var ratings = IgdbGameDetails.AgeRatings(IgdbGame() with
        {
            AgeRatings =
            [
                new IgdbAgeRating(new IgdbNamed("PEGI"), new IgdbAgeRatingCategory("7")),
                new IgdbAgeRating(new IgdbNamed("CLASS_IND"), new IgdbAgeRatingCategory("L")),
                new IgdbAgeRating(new IgdbNamed("ESRB"), null),
                new IgdbAgeRating(null, new IgdbAgeRatingCategory("B")),
                new IgdbAgeRating(new IgdbNamed("PEGI"), new IgdbAgeRatingCategory("12"))
            ]
        });

        ratings.Select(r => r.ToString()).Should().Equal("PEGI 7", "ClassInd L");
    }

    [Theory]
    [InlineData("europe", Region.Pal)]
    [InlineData("australia", Region.Pal)]
    [InlineData("north_america", Region.NtscU)]
    [InlineData("brazil", Region.NtscU)]
    [InlineData("japan", Region.NtscJ)]
    [InlineData("korea", Region.Other)]
    [InlineData("worldwide", null)]
    public void Igdb_MapsReleaseRegionsToVideoStandards(string igdbRegion, Region? expected) =>
        IgdbGameDetails.Region(new IgdbReleaseRegion(igdbRegion)).Should().Be(expected);

    [Theory]
    [InlineData(Region.Pal, "PEGI 7")]
    [InlineData(Region.NtscU, "ESRB E10+")]
    [InlineData(Region.NtscJ, "CERO B")]
    [InlineData(Region.RegionFree, "PEGI 7")]
    public void AgeRatingFor_PicksTheBoardOfTheRegion(Region region, string expected)
    {
        var game = CatalogGame(Rating("CERO", "B"), Rating("ESRB", "E10+"), Rating("USK", "6"), Rating("PEGI", "7"));

        game.AgeRatingFor(region).Should().Be(expected);
    }

    [Fact]
    public void AgeRatingFor_FallsBackWithinPal_ButNotToOtherRegions()
    {
        var game = CatalogGame(Rating("USK", "12"), Rating("ESRB", "T"));

        game.AgeRatingFor(Region.Pal).Should().Be("USK 12");
        game.AgeRatingFor(Region.NtscJ).Should().BeNull();
    }

    [Fact]
    public void ReleaseDateFor_PicksTheFirstReleaseInTheRegion()
    {
        var platform = Platform(
            Release(Region.NtscJ, new ReleaseDate(1996, 6, 23)),
            Release(Region.NtscU, new ReleaseDate(1996, 9, 29)),
            Release(Region.Pal, new ReleaseDate(1997, 3, 1)),
            Release(Region.Pal, new ReleaseDate(1999)));

        platform.ReleaseDateFor(Region.Pal).Should().Be(new ReleaseDate(1997, 3, 1));
        platform.ReleaseDateFor(Region.NtscJ).Should().Be(new ReleaseDate(1996, 6, 23));
        platform.ReleaseDateFor(Region.RegionFree).Should().Be(new ReleaseDate(1996, 6, 23));
    }

    [Fact]
    public void ReleaseDateFor_CountsWorldwideReleasesForEveryRegion()
    {
        var platform = Platform(Release(null, new ReleaseDate(2017, 10, 27)), Release(Region.NtscU, new ReleaseDate(2022, 10, 26)));

        platform.ReleaseDateFor(Region.Pal).Should().Be(new ReleaseDate(2017, 10, 27));
        platform.ReleaseDateFor(Region.NtscU).Should().Be(new ReleaseDate(2017, 10, 27));
    }

    [Fact]
    public void ReleaseDateFor_IsEmpty_WhenItDidNotComeOutInTheRegion()
    {
        Platform(Release(Region.NtscJ, new ReleaseDate(1995))).ReleaseDateFor(Region.Pal).Should().BeNull();
        Platform().ReleaseDateFor(Region.RegionFree).Should().BeNull();
    }

    private static IgdbGame IgdbGame() =>
        new(Id: 1, Name: "Super Mario 64", VersionTitle: null, FirstReleaseDate: null, ReleaseDates: null, Cover: null,
            Platforms: null, Genres: null);

    private static CatalogAgeRatingDto Rating(string organization, string rating) =>
        new() { Organization = organization, Rating = rating };

    private static CatalogGameDto CatalogGame(params CatalogAgeRatingDto[] ratings) =>
        new() { Id = 1, Name = "Super Mario Odyssey", AgeRatings = ratings };

    private static CatalogReleaseDto Release(Region? region, ReleaseDate date) => new() { Region = region, ReleaseDate = date };

    private static CatalogPlatformDto Platform(params CatalogReleaseDto[] releases) =>
        new() { Name = "Nintendo 64", Releases = releases };
}
