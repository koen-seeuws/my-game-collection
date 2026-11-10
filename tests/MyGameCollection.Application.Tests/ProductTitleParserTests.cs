using MyGameCollection.Application.Features.Barcodes.Common;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Tests;

public class ProductTitleParserTests
{
    private static readonly Guid Snes = Guid.NewGuid();
    private static readonly Guid N64 = Guid.NewGuid();
    private static readonly Guid Gb = Guid.NewGuid();
    private static readonly Guid Gba = Guid.NewGuid();
    private static readonly Guid Ps1 = Guid.NewGuid();
    private static readonly Guid Ps2 = Guid.NewGuid();
    private static readonly Guid Sms = Guid.NewGuid();

    private static readonly PlatformName[] Platforms =
    [
        new(Snes, "Super Nintendo"), new(Snes, "SNES"), new(Snes, "Super Nintendo Entertainment System"),
        new(N64, "Nintendo 64"), new(N64, "N64"),
        new(Gb, "Game Boy"), new(Gb, "GB"),
        new(Gba, "Game Boy Advance"), new(Gba, "GBA"),
        new(Ps1, "PlayStation"), new(Ps1, "PS1"),
        new(Ps2, "PlayStation 2"), new(Ps2, "PS2"),
        new(Sms, "Master System"), new(Sms, "Sega Master System"),
    ];

    [Theory]
    [InlineData("Super Mario 64 - Nintendo 64", "Super Mario 64")]
    [InlineData("The Legend of Zelda: A Link to the Past (SNES) PAL", "The Legend of Zelda: A Link to the Past PAL")]
    [InlineData("Gran Turismo 3 A-Spec (PS2)", "Gran Turismo 3 A-Spec")]
    [InlineData("Pokemon Ruby Version - Game Boy Advance", "Pokemon Ruby")]
    [InlineData("Metal Gear Solid [PlayStation] Video Game", "Metal Gear Solid")]
    public void CleansTitle(string raw, string expected) =>
        Assert.Equal(expected, ProductTitleParser.Parse(raw, null, Platforms).Title);

    [Theory]
    [InlineData("Pokemon Ruby - Game Boy Advance", "GBA")]
    [InlineData("Tetris for Game Boy", "GB")]
    [InlineData("Gran Turismo 3 (PlayStation 2)", "PS2")]
    [InlineData("Crash Bandicoot PS1", "PS1")]
    [InlineData("Sonic the Hedgehog - Sega Master System", "SMS")]
    [InlineData("Some Random Thing", null)]
    public void DetectsMostSpecificPlatform(string raw, string? expected)
    {
        var expectedId = expected switch
        {
            "GBA" => Gba, "GB" => Gb, "PS2" => Ps2, "PS1" => Ps1, "SMS" => Sms, _ => (Guid?)null
        };
        Assert.Equal(expectedId, ProductTitleParser.Parse(raw, null, Platforms).PlatformId);
    }

    [Fact]
    public void PlatformNameContainingSystemIsStillAGame()
    {
        var parsed = ProductTitleParser.Parse("Super Mario World (Super Nintendo Entertainment System)", "Video Games", Platforms);
        Assert.Equal(ItemType.Game, parsed.Type);
        Assert.Equal(Snes, parsed.PlatformId);
    }

    [Fact]
    public void DetectsConsoleHardware()
    {
        var parsed = ProductTitleParser.Parse("Nintendo 64 Console - Charcoal Grey", "Electronics > Video Game Consoles", Platforms);
        Assert.Equal(ItemType.Console, parsed.Type);
        Assert.Equal(N64, parsed.PlatformId);
    }

    [Theory]
    [InlineData("Final Fantasy VII (PAL)", Region.Pal)]
    [InlineData("Final Fantasy VII NTSC-J Japan import", Region.NtscJ)]
    [InlineData("Final Fantasy VII - USA", Region.NtscU)]
    [InlineData("Among Us - Rated E", null)] // short words in normal text must not count
    public void DetectsRegionFromText(string raw, Region? expected) =>
        Assert.Equal(expected, ProductTitleParser.Parse(raw, null, Platforms).Region);
}
