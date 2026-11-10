using Microsoft.EntityFrameworkCore;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Features.Dashboard;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Domain.Enums;
using static MyGameCollection.Application.Tests.CollectionScenario;

namespace MyGameCollection.Application.Tests;

/// <summary>
/// Runs every repository query against a real PostgreSQL database, to catch LINQ that the InMemory provider
/// accepts but Npgsql can't translate. Only runs when MGC_TEST_POSTGRES holds a connection string, e.g.
/// docker run --rm -d -p 55432:5432 -e POSTGRES_PASSWORD=test postgres:17-alpine
/// MGC_TEST_POSTGRES="Host=localhost;Port=55432;Username=postgres;Password=test;Database=mgc_test" dotnet test
/// </summary>
public class PostgresQueryTests
{
    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable("MGC_TEST_POSTGRES");

    [Fact]
    public async Task AllQueriesTranslateOnPostgres()
    {
        if (string.IsNullOrEmpty(ConnectionString)) return; // not configured: skip

        await using var host = new TestHost(o => o.UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention());
        await using (var db = await host.CreateDbContextAsync())
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var sender = host.Sender;
        var s = await SeedAsync(sender);
        await sender.Send(new AddPhotoCommand(s.SuperMetroid, new PhotoUpload(new MemoryStream([1]), new MemoryStream([2]), "image/jpeg", "front.jpg", 1)));

        var full = await Search(sender, new ItemFilter
        {
            Search = "metroid", Type = ItemType.Game, PlatformIds = [s.Snes], Regions = [Region.Pal],
            Statuses = [ItemStatus.Owned], MinimumCondition = ItemCondition.Good, CompleteOnly = true, HasPhotos = true,
            SortBy = ItemSortField.PurchasePrice, Descending = true
        });
        Assert.NotNull(Assert.Single(full.Items).PrimaryPhotoId);

        // Condition is stored as text: "Good or better" must not compare strings alphabetically.
        var goodOrBetter = await Search(sender, new ItemFilter { MinimumCondition = ItemCondition.Good });
        Assert.Equal(3, goodOrBetter.TotalCount);
        Assert.DoesNotContain(goodOrBetter.Items, i => i.Title == "Donkey Kong Country");

        foreach (var sort in Enum.GetValues<ItemSortField>())
        foreach (var descending in new[] { false, true })
            Assert.Equal(4, (await Search(sender, new ItemFilter { SortBy = sort, Descending = descending })).TotalCount);

        Assert.Equal(3, (await Search(sender, new ItemFilter { HasPhotos = false })).TotalCount);
        Assert.Equal(3, (await Search(sender, new ItemFilter { Search = "super" })).TotalCount);
        Assert.Single(await sender.Send(new FindItemsByBarcodeQuery("0045496830137")));

        var dashboard = await sender.Send(new GetDashboardQuery());
        Assert.Equal((2, 1, 1), (dashboard.Statistics.GameCount, dashboard.Statistics.ConsoleCount, dashboard.Statistics.WishlistCount));
        Assert.Single(dashboard.Statistics.TopPlatforms); // Mega Drive only has a wishlist item

        var platforms = await sender.Send(new GetPlatformsQuery());
        Assert.Equal((2, 1), platforms.Where(p => p.Id == s.Snes).Select(p => (p.GameCount, p.ConsoleCount)).Single());

        var edit = await sender.Send(new GetItemForEditQuery(s.SuperMetroid));
        edit.Notes = "Saved via Postgres";
        await sender.Send(new UpdateItemCommand(s.SuperMetroid, edit));
        Assert.Equal("Saved via Postgres", (await sender.Send(new GetItemDetailsQuery(s.SuperMetroid))).Notes);

        await sender.Send(new DeleteItemCommand(s.SuperMetroid));
        Assert.Equal(3, (await Search(sender, new ItemFilter())).TotalCount);
    }
}
