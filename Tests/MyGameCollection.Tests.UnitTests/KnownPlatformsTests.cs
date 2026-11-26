using MyGameCollection.Presentation.BlazorServer.Models;

namespace MyGameCollection.Tests.UnitTests;

public class KnownPlatformsTests
{
    [Theory]
    [InlineData("Sega Mega Drive/Genesis", "Genesis/MegaDrive", "Mega Drive")]
    [InlineData("Super Nintendo Entertainment System", "SNES", "Super Nintendo")]
    [InlineData("Nintendo GameCube", "NGC", "GameCube")]
    [InlineData("PlayStation 2", "PS2", "PlayStation 2")]
    [InlineData("Some Obscure Handheld", null, "Some Obscure Handheld")]
    public void Catalog_names_map_to_the_well_known_names(string name, string? abbreviation, string expected) =>
        Assert.Equal(expected, KnownPlatforms.Match([], name, abbreviation));

    [Fact]
    public void The_collections_own_name_for_a_system_wins()
    {
        Assert.Equal("Sega Genesis", KnownPlatforms.Match(["Sega Genesis"], "Sega Mega Drive/Genesis", "Genesis/MegaDrive"));
        Assert.Equal("PS2", KnownPlatforms.Match(["PS2"], "PlayStation 2"));
    }

    [Theory]
    [InlineData("Gran Turismo 4 (Sony PlayStation 2)", "PlayStation 2")]
    [InlineData("Halo 3 - Xbox 360 - PAL", "Xbox 360")]
    [InlineData("Super Mario World SNES Cartridge", "Super Nintendo")]
    [InlineData("Some board game", null)]
    public void Platforms_are_detected_in_product_titles(string title, string? expected) =>
        Assert.Equal(expected, KnownPlatforms.DetectIn(title, []));
}
