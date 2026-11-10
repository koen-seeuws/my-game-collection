using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Barcodes.Common;

/// <summary>
/// Guesses the release region from the GS1 prefix of an EAN/UPC barcode.
/// The prefix tells where the company registered its barcode, which for games is a decent proxy for the market.
/// </summary>
public static class BarcodeRegionDetector
{
    public static Region Detect(string barcode)
    {
        if (barcode.Length == 12) // UPC-A: US / Canada
            return Region.NtscU;

        if (barcode.Length != 13 || !barcode.All(char.IsDigit))
            return Region.Unknown;

        var prefix = int.Parse(barcode[..3]);
        return prefix switch
        {
            <= 139 => Region.NtscU,                  // 000-139 US & Canada
            >= 450 and <= 459 => Region.NtscJ,       // Japan
            >= 490 and <= 499 => Region.NtscJ,       // Japan
            >= 300 and <= 449 => Region.Pal,         // France, Germany
            >= 500 and <= 509 => Region.Pal,         // UK
            >= 520 and <= 569 => Region.Pal,         // Greece, Belgium/Lux, Portugal, Iceland, Denmark ...
            >= 570 and <= 579 => Region.Pal,         // Denmark
            >= 590 and <= 599 => Region.Pal,         // Poland ...
            >= 640 and <= 649 => Region.Pal,         // Finland
            >= 700 and <= 709 => Region.Pal,         // Norway
            >= 730 and <= 739 => Region.Pal,         // Sweden
            >= 760 and <= 769 => Region.Pal,         // Switzerland
            >= 800 and <= 849 => Region.Pal,         // Italy, Spain
            >= 870 and <= 879 => Region.Pal,         // Netherlands
            >= 900 and <= 919 => Region.Pal,         // Austria
            >= 930 and <= 939 => Region.Pal,         // Australia
            >= 940 and <= 949 => Region.Pal,         // New Zealand
            _ => Region.Unknown
        };
    }
}
