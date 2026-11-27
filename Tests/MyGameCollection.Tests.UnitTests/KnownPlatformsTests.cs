using MyGameCollection.Presentation.BlazorServer.Models;

namespace MyGameCollection.Tests.UnitTests;

public class KnownPlatformsTests
{
    [Theory]
    [InlineData("Sega Mega Drive/Genesis", "Genesis/MegaDrive", "Mega Drive")]
    [InlineData("Super Nintendo Entertainment System", "SNES", "Super Nintendo")]
    [InlineData("Nintendo GameCube", "NGC", "GameCube")]
    [InlineData("PlayStation 2", "PS2", "PlayStation 2")]
    [InlineData("PlayStation 5", "PS5", "PlayStation 5")]
    [InlineData("Nintendo Switch 2", "Switch 2", "Nintendo Switch 2")]
    [InlineData("Xbox Series X|S", "Series X|S", "Xbox Series X|S")]
    [InlineData("Some Obscure Handheld", null, "Some Obscure Handheld")]
    public void Catalog_names_map_to_the_well_known_names(string name, string? abbreviation, string expected) =>
        KnownPlatforms.Match([], name, abbreviation).Should().Be(expected);

    [Fact]
    public void The_collections_own_name_for_a_system_wins()
    {
        KnownPlatforms.Match(["Sega Genesis"], "Sega Mega Drive/Genesis", "Genesis/MegaDrive").Should().Be("Sega Genesis");
        KnownPlatforms.Match(["PS2"], "PlayStation 2").Should().Be("PS2");
    }
}
