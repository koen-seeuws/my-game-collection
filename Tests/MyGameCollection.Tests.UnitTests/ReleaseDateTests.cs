using System.Globalization;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Tests.UnitTests.GameCollectionBuilder;

namespace MyGameCollection.Tests.UnitTests;

public class ReleaseDateTests
{
    [Theory]
    [InlineData(1991, null, null)]
    [InlineData(1991, 6, null)]
    [InlineData(1991, 6, 23)]
    [InlineData(1992, 2, 29)]
    public void A_year_month_or_day_that_exists_is_valid(int year, int? month, int? day) =>
        new ReleaseDate(year, month, day).Problem.Should().BeNull();

    [Theory]
    [InlineData(1949, null, null)]
    [InlineData(2101, null, null)]
    [InlineData(1991, 0, null)]
    [InlineData(1991, 13, null)]
    [InlineData(1991, null, 23)]
    [InlineData(1991, 6, 31)]
    [InlineData(1991, 2, 29)]
    [InlineData(1991, 6, 0)]
    public void A_day_or_month_that_does_not_exist_is_invalid(int year, int? month, int? day) =>
        new ReleaseDate(year, month, day).Problem.Should().NotBeNull();

    [Fact]
    public void Release_dates_sort_chronologically_with_a_year_before_its_months_and_days()
    {
        ReleaseDate[] dates = [new(1992), new(1991, 6, 23), new(1991, 6), new(1991), new(1991, 1, 5)];

        dates.Order().Should().Equal(new(1991), new(1991, 1, 5), new(1991, 6), new(1991, 6, 23), new(1992));
    }

    [Fact]
    public void A_release_date_reads_as_precisely_as_it_is_known()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-BE");
        try
        {
            new ReleaseDate(1991).ToString().Should().Be("1991");
            new ReleaseDate(1991, 6).ToString().Should().Be("Jun 1991");
            new ReleaseDate(1991, 6, 23).ToString().Should().Be("23 Jun 1991");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void AddGame_WithANonExistingDay_Throws()
    {
        var collection = Empty();

        collection.Invoking(c => c.AddGame("Sonic the Hedgehog", "Mega Drive", releaseDate: new ReleaseDate(1991, 2, 30)))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateGame_CanMakeTheReleaseDateMoreOrLessPrecise()
    {
        var collection = Empty();
        var key = collection.AddGame("Sonic the Hedgehog", "Mega Drive", releaseDate: new ReleaseDate(1991));

        collection.UpdateGame(key, "Sonic the Hedgehog", "Mega Drive", releaseDate: new ReleaseDate(1991, 6, 23));
        collection.Games.Single().ReleaseDate.Should().Be(new ReleaseDate(1991, 6, 23));

        collection.UpdateGame(key, "Sonic the Hedgehog", "Mega Drive", releaseDate: null);
        collection.Games.Single().ReleaseDate.Should().BeNull();
    }

    [Fact]
    public void A_command_with_a_non_existing_day_reports_why()
    {
        var result = new AddGameCommandInputValidator().Validate(new AddGameCommand
        {
            Title = "Sonic the Hedgehog", Platform = "Mega Drive", ReleaseDate = new ReleaseDate(1991, 6, 31)
        });

        var error = result.Errors.Should().ContainSingle().Subject;
        error.PropertyName.Should().Be("ReleaseDate");
        error.ErrorMessage.Should().Be("That day does not exist in the release month.");
    }
}
