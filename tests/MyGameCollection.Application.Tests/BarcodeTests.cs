using MyGameCollection.Application.Features.Barcodes.Common;
using MyGameCollection.Application.Common;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Tests;

public class BarcodeTests
{
    [Theory]
    [InlineData(" 5030 917-012345 ", "5030917012345")]
    [InlineData("045496590079", "0045496590079")] // UPC-A -> EAN-13
    [InlineData("", null)]
    [InlineData("  ", null)]
    public void Normalize(string? input, string? expected) =>
        Assert.Equal(expected, BarcodeNormalizer.Normalize(input));

    [Theory]
    [InlineData("0045496590079", Region.NtscU)]
    [InlineData("4902370517415", Region.NtscJ)]
    [InlineData("5030917012345", Region.Pal)]   // UK
    [InlineData("5413000000000", Region.Pal)]   // Belgium
    [InlineData("3307210000000", Region.Pal)]   // France
    [InlineData("8710000000000", Region.Pal)]   // Netherlands
    [InlineData("ABC", Region.Unknown)]
    public void DetectRegionFromPrefix(string barcode, Region expected) =>
        Assert.Equal(expected, BarcodeRegionDetector.Detect(barcode));
}
