using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Queries.FindConsoleByBarcode;
using MyGameCollection.Application.Abstractions.Queries.FindGameByBarcode;
using MyGameCollection.Application.Abstractions.Services;
using MyGameCollection.Application.Abstractions.Services.Barcodes;
using MyGameCollection.Application.Abstractions.Services.GameCatalog;
using MyGameCollection.Application.Business.QueryHandlers.FindConsoleByBarcode;
using MyGameCollection.Application.Business.QueryHandlers.FindGameByBarcode;
using MyGameCollection.Infrastructure.ScanDex;

namespace MyGameCollection.Tests.UnitTests;

/// <summary>Finding a game or console by the barcode on its box: ScanDex gives the IGDB ids, IGDB the details.</summary>
public class BarcodeLookupTests
{
    private const string Matched = """
        { "id": 87859, "source": "import",
          "igdb_metadata": { "id": 26758, "name": "Super Mario Odyssey", "platform": { "id": 130, "name": "Nintendo Switch" } } }
        """;

    [Theory]
    [InlineData("0045496590741", "0045496590741")]
    [InlineData(" 0 45496-59074 1 ", "045496590741")]
    [InlineData("12345678", "12345678")]
    public void Barcode_ReadsDigitsIgnoringSpacesAndDashes(string text, string expected)
    {
        Barcode.TryParse(text, out var barcode).Should().BeTrue();
        barcode!.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")]
    [InlineData("123456789012345")]
    [InlineData("Super Mario 64")]
    [InlineData("1942")]
    public void Barcode_RefusesWhatIsNotABarcode(string? text)
    {
        Barcode.TryParse(text, out _).Should().BeFalse();
        FluentActions.Invoking(() => Barcode.Parse(text)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ScanDex_ReturnsTheMatchedGameAndPlatform()
    {
        var handler = new StubHandler(HttpStatusCode.OK, Matched);

        var match = await ScanDex(handler).FindAsync(Barcode.Parse("0045496590741"));

        match.Should().BeEquivalentTo(new BarcodeMatchDto
        {
            CatalogGameId = 26758, GameName = "Super Mario Odyssey", CatalogPlatformId = 130, PlatformName = "Nintendo Switch"
        });
        handler.Request!.RequestUri.Should().Be("https://scandex.gamery.app/api/v2/lookup?value=0045496590741");
        handler.Request.Headers.GetValues("Authorization").Should().Equal("secret-token");
    }

    [Fact]
    public async Task ScanDex_ReadsTheOlderFlatPlatformId()
    {
        var match = await ScanDex(new StubHandler(HttpStatusCode.OK,
            """{ "id": 1, "title": "God of War Ragnarök", "igdb_metadata": { "id": 112875, "platform_id": 167 } }""")
        ).FindAsync(Barcode.Parse("0711719577966"));

        match!.CatalogGameId.Should().Be(112875);
        match.CatalogPlatformId.Should().Be(167);
        match.PlatformName.Should().BeNull();
    }

    [Fact]
    public async Task ScanDex_FindsNothingForAnUnknownBarcode() =>
        (await ScanDex(new StubHandler(HttpStatusCode.NotFound, """{ "message": "This barcode does not exist yet." }"""))
            .FindAsync(Barcode.Parse("12345678"))).Should().BeNull();

    [Fact]
    public async Task ScanDex_FindsNothingForABarcodeNotMatchedYet() =>
        (await ScanDex(new StubHandler(HttpStatusCode.OK,
                """{ "id": 178182, "source": "import", "status": "unmatched", "igdb_metadata": null }"""))
            .FindAsync(Barcode.Parse("12345678"))).Should().BeNull();

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ScanDex_ReportsFailuresAsUnavailable(HttpStatusCode status) =>
        await FluentActions.Awaiting(() => ScanDex(new StubHandler(status, "{}")).FindAsync(Barcode.Parse("12345678")))
            .Should().ThrowAsync<CatalogUnavailableException>();

    [Fact]
    public async Task ScanDex_NeedsAnAccessToken()
    {
        var service = new ScanDexBarcodeCatalogService(new HttpClient(new StubHandler(HttpStatusCode.OK, Matched)),
            Options.Create(new ScanDexOptions()));

        await FluentActions.Awaiting(() => service.FindAsync(Barcode.Parse("12345678")))
            .Should().ThrowAsync<CatalogUnavailableException>().WithMessage("*ScanDex:AccessToken*");
    }

    [Fact]
    public async Task FindGame_FetchesTheMatchedGameFromTheCatalog()
    {
        var catalog = new FakeCatalog();
        var handler = new FindGameByBarcodeQueryHandler(new FakeBarcodes(Match), catalog);

        var found = await handler.HandleAsync(new FindGameByBarcodeQuery { Barcode = "0045496590741" }, default);

        found!.Game.Name.Should().Be("Super Mario Odyssey");
        found.PlatformName.Should().Be("Nintendo Switch");
        catalog.RequestedGameId.Should().Be(26758);
    }

    [Fact]
    public async Task FindGame_FindsNothingForAnUnknownBarcode()
    {
        var catalog = new FakeCatalog();
        var handler = new FindGameByBarcodeQueryHandler(new FakeBarcodes(null), catalog);

        (await handler.HandleAsync(new FindGameByBarcodeQuery { Barcode = "12345678" }, default)).Should().BeNull();
        catalog.RequestedGameId.Should().BeNull();
    }

    [Fact]
    public async Task FindGame_RefusesWhatIsNotABarcode() =>
        await FluentActions.Awaiting(() => new FindGameByBarcodeQueryHandler(new FakeBarcodes(Match), new FakeCatalog())
                .HandleAsync(new FindGameByBarcodeQuery { Barcode = "Mario" }, default))
            .Should().ThrowAsync<ArgumentException>();

    [Fact]
    public async Task FindConsole_FetchesTheMatchedPlatformFromTheCatalog()
    {
        var catalog = new FakeCatalog();
        var handler = new FindConsoleByBarcodeQueryHandler(new FakeBarcodes(Match), catalog);

        var console = await handler.HandleAsync(new FindConsoleByBarcodeQuery { Barcode = "0045496590741" }, default);

        console!.Name.Should().Be("Nintendo Switch");
        catalog.RequestedConsoleId.Should().Be(130);
    }

    [Fact]
    public async Task FindConsole_FindsNothingWithoutAPlatform() =>
        (await new FindConsoleByBarcodeQueryHandler(new FakeBarcodes(Match with { CatalogPlatformId = null }), new FakeCatalog())
            .HandleAsync(new FindConsoleByBarcodeQuery { Barcode = "0045496590741" }, default)).Should().BeNull();

    private static readonly BarcodeMatchDto Match = new()
    {
        CatalogGameId = 26758, GameName = "Super Mario Odyssey", CatalogPlatformId = 130, PlatformName = "Nintendo Switch"
    };

    private static ScanDexBarcodeCatalogService ScanDex(HttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(new ScanDexOptions { AccessToken = "secret-token" }));

    private sealed class StubHandler(HttpStatusCode status, string json) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FakeBarcodes(BarcodeMatchDto? match) : IBarcodeCatalogService
    {
        public Task<BarcodeMatchDto?> FindAsync(Barcode barcode, CancellationToken cancellationToken = default) =>
            Task.FromResult(match);
    }

    private sealed class FakeCatalog : IGameCatalogService
    {
        public long? RequestedGameId { get; private set; }
        public long? RequestedConsoleId { get; private set; }

        public Task<CatalogGameDto?> GetGameAsync(long gameId, CancellationToken cancellationToken = default)
        {
            RequestedGameId = gameId;
            return Task.FromResult<CatalogGameDto?>(new CatalogGameDto { Id = gameId, Name = "Super Mario Odyssey" });
        }

        public Task<CatalogConsoleDto?> GetConsoleAsync(long consoleId, CancellationToken cancellationToken = default)
        {
            RequestedConsoleId = consoleId;
            return Task.FromResult<CatalogConsoleDto?>(new CatalogConsoleDto { Id = consoleId, Name = "Nintendo Switch" });
        }

        public Task<IReadOnlyList<CatalogGameDto>> SearchGamesAsync(string search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogGameEditionDto>> GetGameEditionsAsync(long gameId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogAddOnDto>> GetGameAddOnsAsync(long gameId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogAddOnDto>> SearchAddOnsAsync(string search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<CatalogConsoleDto>> SearchConsolesAsync(string search, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
