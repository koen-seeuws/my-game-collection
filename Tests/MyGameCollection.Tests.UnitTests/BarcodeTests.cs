using MyGameCollection.Core.Domain.Abstractions;
using MyGameCollection.Infrastructure.Barcodes;

namespace MyGameCollection.Tests.UnitTests;

public class BarcodeTests
{
    [Theory]
    [InlineData("4006381333931", "4006381333931")] // EAN-13
    [InlineData("4006 3813 3393 1", "4006381333931")] // typed with spaces
    [InlineData("036000291452", "036000291452")] // UPC-A
    [InlineData("96385074", "96385074")] // EAN-8
    [InlineData("01234565", "012345000065")] // UPC-E, expanded to UPC-A
    public void Valid_barcodes_are_normalized(string input, string expected)
    {
        Assert.True(Gtin.TryNormalize(input, out var barcode));
        Assert.Equal(expected, barcode);
    }

    [Theory]
    [InlineData("4006381333932")] // wrong check digit
    [InlineData("12345")]
    [InlineData("Super Mario 64")]
    [InlineData("")]
    public void Invalid_barcodes_are_rejected(string input) =>
        Assert.False(Gtin.TryNormalize(input, out _));

    [Theory]
    [InlineData("4902370517330", Region.NtscJ)] // Japan
    [InlineData("5045958000103", Region.Pal)] // United Kingdom
    [InlineData("4006381333931", Region.Pal)] // Germany
    [InlineData("0045496420253", null)] // Nintendo's US prefix, used on European releases too
    [InlineData("036000291452", null)]
    public void Region_is_guessed_from_the_prefix(string barcode, Region? expected) =>
        Assert.Equal(expected, Gtin.GuessRegion(barcode));

    [Theory]
    [InlineData("Super Mario 64 (Nintendo 64, 1996)", "Super Mario 64")]
    [InlineData("Halo 3 - Xbox 360", "Halo 3")]
    [InlineData("The Legend of Zelda: Twilight Princess - Nintendo Wii [PAL]", "The Legend of Zelda: Twilight Princess")]
    [InlineData("Gran Turismo 4 PS2 Game", "Gran Turismo 4")]
    [InlineData("Metal Gear Solid Video Game for PlayStation", "Metal Gear Solid")]
    [InlineData("Wii Sports", "Wii Sports")]
    [InlineData("Spider-Man 2 - PlayStation 2 - Brand New Sealed", "Spider-Man 2")]
    [InlineData("Batman - Arkham Asylum (PS3)", "Batman - Arkham Asylum")]
    [InlineData("Nintendo Switch/pal-hac Mario Kart 8 Deluxe", "Mario Kart 8 Deluxe")]
    [InlineData("Sony PlayStation 2 Ico SCES-50760", "Ico")]
    [InlineData("PlayStation All-Stars Battle Royale", "PlayStation All-Stars Battle Royale")]
    public void Product_titles_become_search_terms(string productName, string expected) =>
        Assert.Equal(expected, ProductTitle.ToSearchTerm(productName));
}
