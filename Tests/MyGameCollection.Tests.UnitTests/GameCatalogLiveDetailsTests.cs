using System.Globalization;
using MyGameCollection.Infrastructure.IGDB;
using MyGameCollection.Infrastructure.IGDB.QueryHandlers.GetGameCatalogDetails;
using MyGameCollection.Presentation.BlazorServer.Models;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>A game's live details from IGDB (rating, summary, time to beat, screenshots) and how they are shown.</summary>
public class GameCatalogLiveDetailsTests
{
    [Fact]
    public void Igdb_MapsLiveDetails()
    {
        var details = GetGameCatalogDetailsQueryHandler.ToDetails(
            new IgdbGameLiveDetails(1074, " Mario is super in a whole new way! ", 88.7, 2102,
                [new IgdbImage("sc8e2l"), new IgdbImage(null), new IgdbImage("sc8e2o")],
                "https://www.igdb.com/games/super-mario-64"),
            new IgdbTimeToBeat(Hastily: 30300, Normally: 46800, Completely: 50520, Count: 8));

        details.Summary.Should().Be("Mario is super in a whole new way!");
        details.Rating.Should().Be(88.7);
        details.RatingCount.Should().Be(2102);
        details.TimeToBeat!.Normal.Should().Be(TimeSpan.FromSeconds(46800));
        details.TimeToBeat.Count.Should().Be(8);
        details.Screenshots.Select(s => s.ThumbnailUrl).Should().Equal(
            "https://images.igdb.com/igdb/image/upload/t_screenshot_med/sc8e2l.png",
            "https://images.igdb.com/igdb/image/upload/t_screenshot_med/sc8e2o.png");
        details.Screenshots[0].Url.Should().Be("https://images.igdb.com/igdb/image/upload/t_1080p/sc8e2l.png");
        details.CatalogUrl.Should().Be("https://www.igdb.com/games/super-mario-64");
    }

    [Fact]
    public void Igdb_LeavesOutWhatNobodyReported()
    {
        var details = GetGameCatalogDetailsQueryHandler.ToDetails(
            new IgdbGameLiveDetails(1, "  ", TotalRating: 50, TotalRatingCount: 0, Screenshots: null, Url: null),
            new IgdbTimeToBeat(Hastily: null, Normally: 0, Completely: null, Count: 0));

        details.Summary.Should().BeNull();
        details.Rating.Should().BeNull();
        details.TimeToBeat.Should().BeNull();
        details.Screenshots.Should().BeEmpty();
    }

    [Fact]
    public void Igdb_KeepsAFewScreenshots()
    {
        var screenshots = Enumerable.Range(1, 20).Select(i => new IgdbImage($"sc{i}")).ToList();

        GetGameCatalogDetailsQueryHandler.ToDetails(new IgdbGameLiveDetails(1, null, null, null, screenshots, null), null)
            .Screenshots.Should().HaveCount(8);
    }

    [Theory]
    [InlineData(20, "20 min")]
    [InlineData(50, "1 h")]
    [InlineData(13 * 60, "13 h")]
    [InlineData(12 * 60 + 40, "12.5 h")]
    [InlineData(68 * 60 + 39, "68.5 h")]
    public void Hours_RoundsToTheNearestHalfHour(int minutes, string expected)
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            DisplayNames.Hours(TimeSpan.FromMinutes(minutes)).Should().Be(expected);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }
}
