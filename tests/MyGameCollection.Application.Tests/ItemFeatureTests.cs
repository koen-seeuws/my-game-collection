using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Common;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Application.Abstractions.Features.Dashboard;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Features.Photos;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Domain.Enums;
using static MyGameCollection.Application.Tests.CollectionScenario;

namespace MyGameCollection.Application.Tests;

public class ItemFeatureTests
{
    [Fact]
    public async Task FiltersByTypePlatformStatusConditionAndCompleteness()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        Assert.Equal(4, (await Search(host.Sender, new ItemFilter())).TotalCount);
        Assert.Equal(1, (await Search(host.Sender, new ItemFilter { Type = ItemType.Console })).TotalCount);
        Assert.Equal(3, (await Search(host.Sender, new ItemFilter { PlatformIds = [s.Snes] })).TotalCount);
        Assert.Equal(1, (await Search(host.Sender, new ItemFilter { Statuses = [ItemStatus.Wishlist] })).TotalCount);
        Assert.Equal("Super Metroid", Assert.Single((await Search(host.Sender, new ItemFilter { CompleteOnly = true })).Items).Title);

        // "Good or better": Super Metroid (Very good) + 2 items with the default "Good"; DKC is only "Fair".
        var goodOrBetter = await Search(host.Sender, new ItemFilter { MinimumCondition = ItemCondition.Good });
        Assert.DoesNotContain(goodOrBetter.Items, i => i.Title == "Donkey Kong Country");
        Assert.Equal(3, goodOrBetter.TotalCount);
    }

    [Fact]
    public async Task SearchesTitlesAndPlatformNamesAndPages()
    {
        await using var host = TestHost.InMemory();
        await SeedAsync(host.Sender);

        // Matches titles *and* platform names: Donkey Kong Country is on "Super Nintendo".
        Assert.Equal(3, (await Search(host.Sender, new ItemFilter { Search = "SUPER" })).TotalCount);
        Assert.Equal(1, (await Search(host.Sender, new ItemFilter { Search = "metroid" })).TotalCount);

        var page = await Search(host.Sender, new ItemFilter { PageSize = 2, Page = 1, SortBy = ItemSortField.Title });
        Assert.Equal(4, page.TotalCount);
        Assert.Equal(["Super Metroid", "Super Nintendo PAL"], page.Items.Select(i => i.Title));
    }

    [Fact]
    public async Task FindsByNormalizedBarcode()
    {
        await using var host = TestHost.InMemory();
        await SeedAsync(host.Sender);

        var found = await host.Sender.Send(new FindItemsByBarcodeQuery("0 045496 830137"));
        Assert.Equal("Super Metroid", Assert.Single(found).Title);
    }

    [Fact]
    public async Task RoundTripsAllFieldsThroughTheDataModel()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        var id = await host.Sender.Send(new CreateItemCommand(new ItemEditModel
        {
            Type = ItemType.Console, Title = " PlayStation ", PlatformId = s.Snes, Region = Region.NtscJ,
            Condition = ItemCondition.Mint, Status = ItemStatus.ForSale, IsSealed = true, Barcode = "4948872000000",
            PurchasePrice = 12.5m, PurchaseDate = new DateTime(2024, 5, 1), EstimatedValue = 40, StorageLocation = "Shelf A",
            Notes = "Note", CoverImageUrl = "https://example.org/a.jpg", Model = "SCPH-1000", SerialNumber = "S1", Edition = "Grey"
        }));

        var d = await host.Sender.Send(new GetItemDetailsQuery(id));
        Assert.Equal(
            (ItemType.Console, "PlayStation", "Super Nintendo", "Nintendo", Region.NtscJ, ItemCondition.Mint, ItemStatus.ForSale, true),
            (d.Type, d.Title, d.PlatformName, d.PlatformManufacturer, d.Region, d.Condition, d.Status, d.IsSealed));
        Assert.Equal(("4948872000000", 12.5m, new DateOnly(2024, 5, 1), 40m, "Shelf A", "Note"),
            (d.Barcode, d.PurchasePrice, d.PurchaseDate, d.EstimatedValue, d.StorageLocation, d.Notes));
        Assert.Equal(("SCPH-1000", "S1", "Grey"), (d.Model, d.SerialNumber, d.Edition));
        Assert.NotEqual(default, d.CreatedAt);
    }

    [Fact]
    public async Task UpdateKeepsTypeAndRejectsTypeChange()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        var model = await host.Sender.Send(new GetItemForEditQuery(s.Sonic));
        model.Notes = "Need the cart";
        model.Publisher = "Sega";
        await host.Sender.Send(new UpdateItemCommand(s.Sonic, model));

        var details = await host.Sender.Send(new GetItemDetailsQuery(s.Sonic));
        Assert.Equal(("Need the cart", "Sega"), (details.Notes, details.Publisher));
        Assert.True(details.UpdatedAt >= details.CreatedAt);

        model.Type = ItemType.Console;
        await Assert.ThrowsAsync<ValidationException>(() => host.Sender.Send(new UpdateItemCommand(s.Sonic, model)));
    }

    [Fact]
    public async Task ValidationBehaviorRejectsInvalidInputBeforeTheHandler()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            host.Sender.Send(new CreateItemCommand(new ItemEditModel { Title = "", PlatformId = s.Snes, ReleaseYear = 1800 })));
        Assert.Contains("Title", ex.Message);
        Assert.Contains("ReleaseYear", ex.Message);

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.Sender.Send(new CreatePlatformCommand(new PlatformEditModel { Name = "", Manufacturer = "" })));
    }

    [Fact]
    public async Task CreateRejectsUnknownPlatformAndDuplicatePlatformNames()
    {
        await using var host = TestHost.InMemory();
        await SeedAsync(host.Sender);

        await Assert.ThrowsAsync<ValidationException>(() =>
            host.Sender.Send(new CreateItemCommand(new ItemEditModel { Title = "Ghost", PlatformId = Guid.NewGuid() })));
        await Assert.ThrowsAsync<ValidationException>(() =>
            host.Sender.Send(new CreatePlatformCommand(new PlatformEditModel { Name = "super nintendo", Manufacturer = "Nintendo" })));
    }

    [Fact]
    public async Task PlatformWithItemsCannotBeDeleted()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        await Assert.ThrowsAsync<ValidationException>(() => host.Sender.Send(new DeletePlatformCommand(s.Snes)));

        var summary = (await host.Sender.Send(new GetPlatformsQuery())).Single(p => p.Id == s.Snes);
        Assert.Equal((2, 1), (summary.GameCount, summary.ConsoleCount));
    }

    [Fact]
    public async Task PhotosAreSyncedToTheDataModelAndFilesCleanedUp()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        static PhotoUpload Upload(string name) => new(new MemoryStream([1, 2]), new MemoryStream([3]), "image/jpeg", name, 2);
        var front = await host.Sender.Send(new AddPhotoCommand(s.SuperMetroid, Upload("front.jpg")));
        var back = await host.Sender.Send(new AddPhotoCommand(s.SuperMetroid, Upload("back.jpg")));
        Assert.Equal(4, host.Storage.Files.Count);

        var details = await host.Sender.Send(new GetItemDetailsQuery(s.SuperMetroid));
        Assert.Equal([front, back], details.Photos.Select(p => p.Id));
        Assert.True(details.Photos[0].IsPrimary);

        await host.Sender.Send(new SetPrimaryPhotoCommand(s.SuperMetroid, back));
        await host.Sender.Send(new DeletePhotoCommand(s.SuperMetroid, front));

        details = await host.Sender.Send(new GetItemDetailsQuery(s.SuperMetroid));
        var remaining = Assert.Single(details.Photos);
        Assert.Equal(back, remaining.Id);
        Assert.True(remaining.IsPrimary);
        Assert.Equal(2, host.Storage.Files.Count);

        await using (var db = await host.CreateDbContextAsync())
            Assert.Single(db.Photos, p => p.ItemId == s.SuperMetroid);

        var thumb = await host.Sender.Send(new GetPhotoFileQuery(back, Thumbnail: true));
        Assert.Equal([3], ((MemoryStream)thumb!.Content).ToArray());

        Assert.Equal(1, (await Search(host.Sender, new ItemFilter { HasPhotos = true })).TotalCount);

        await host.Sender.Send(new DeleteItemCommand(s.SuperMetroid));
        Assert.Empty(host.Storage.Files);
        await Assert.ThrowsAsync<NotFoundException>(() => host.Sender.Send(new GetItemDetailsQuery(s.SuperMetroid)));
    }

    [Fact]
    public async Task RejectsUnsupportedPhotoTypesWithoutWritingFiles()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        await Assert.ThrowsAsync<ValidationException>(() => host.Sender.Send(new AddPhotoCommand(s.SuperMetroid,
            new PhotoUpload(new MemoryStream([1]), new MemoryStream([1]), "image/gif", "a.gif", 1))));
        Assert.Empty(host.Storage.Files);
    }

    [Fact]
    public async Task BarcodeLookupPrefersOwnCollectionThenProviders()
    {
        await using var host = TestHost.InMemory();
        var s = await SeedAsync(host.Sender);

        var owned = await host.Sender.Send(new LookupBarcodeQuery("045496830137"));
        Assert.Equal(("Your collection", "Super Metroid"), (owned.Source, owned.Title));
        Assert.Single(owned.ExistingItems);

        host.Barcodes.Products["5030917012345"] = new BarcodeProduct("Fake", "Sonic 2 - Sega Genesis (PAL)", "Sega");
        var found = await host.Sender.Send(new LookupBarcodeQuery("5030917012345"));
        Assert.Equal(("Sonic 2", s.MegaDrive, Region.Pal, "Sega"), (found.Title, found.PlatformId, found.Region, found.Publisher));

        var missing = await host.Sender.Send(new LookupBarcodeQuery("4902370517415"));
        Assert.False(missing.Found);
        Assert.Equal(Region.NtscJ, missing.Region);

        await Assert.ThrowsAsync<ValidationException>(() => host.Sender.Send(new LookupBarcodeQuery("  ")));
    }

    [Fact]
    public async Task DashboardAggregatesOwnedItems()
    {
        await using var host = TestHost.InMemory();
        await SeedAsync(host.Sender);

        var dashboard = await host.Sender.Send(new GetDashboardQuery());
        var st = dashboard.Statistics;
        Assert.Equal((2, 1, 1, 1, 80m, 90m), (st.GameCount, st.ConsoleCount, st.WishlistCount, st.CompleteCount, st.TotalSpent, st.TotalEstimatedValue));
        Assert.Equal(("Super Nintendo", 2, 1), (st.TopPlatforms[0].Platform, st.TopPlatforms[0].Games, st.TopPlatforms[0].Consoles));
        Assert.Equal(3, dashboard.RecentlyAdded.Count); // wishlist excluded
    }
}
