using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.GameCatalog.Barcodes;

/// <summary>Validation and interpretation of retail barcodes (UPC-A, UPC-E, EAN-8, EAN-13).</summary>
public static class Gtin
{
    /// <summary>
    /// Cleans up a scanned or typed barcode and checks its check digit. UPC-E codes are expanded to UPC-A,
    /// which is what product databases list.
    /// </summary>
    public static bool TryNormalize(string? input, out string barcode)
    {
        barcode = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var digits = new string(input.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
        if (!digits.All(char.IsAsciiDigit))
            return false;

        switch (digits.Length)
        {
            case 12 or 13 when HasValidCheckDigit(digits):
                barcode = digits;
                return true;
            // EAN-8 codes starting with 0 or 1 are reserved for in-store use, so those are read as UPC-E.
            case 8 when ExpandUpcE(digits) is { } upcA && HasValidCheckDigit(upcA):
                barcode = upcA;
                return true;
            case 8 when HasValidCheckDigit(digits):
                barcode = digits; // EAN-8
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// The region hinted at by the barcode's GS1 prefix, or <c>null</c> when it says nothing reliable.
    /// </summary>
    /// <remarks>
    /// The prefix is where the <em>company</em> registered, not the market a copy was sold in: Nintendo, Sony and
    /// Microsoft use their US prefixes on European releases too, so US prefixes are not taken as NTSC-U.
    /// Japanese (45/49) and European/Oceanian prefixes are a decent hint for NTSC-J and PAL.
    /// </remarks>
    public static Region? GuessRegion(string barcode)
    {
        if (barcode.Length is not (12 or 13) || !barcode.All(char.IsAsciiDigit))
            return null;

        var ean13 = barcode.PadLeft(13, '0');
        var prefix = int.Parse(ean13[..3]);

        if (prefix is >= 450 and <= 459 or >= 490 and <= 499)
            return Region.NtscJ;

        return PalPrefixes.Any(range => prefix >= range.From && prefix <= range.To) ? Region.Pal : null;
    }

    // GS1 member organisations in PAL territories (Europe, Australia, New Zealand).
    private static readonly (int From, int To)[] PalPrefixes =
    [
        (300, 379), // France
        (380, 380), (383, 383), (385, 385), (387, 387), // Bulgaria, Slovenia, Croatia, Bosnia
        (400, 440), // Germany
        (500, 509), // United Kingdom
        (520, 521), (528, 531), (535, 535), // Greece, Lebanon, Cyprus, North Macedonia, Malta
        (539, 549), // Ireland, Belgium & Luxembourg
        (560, 560), (569, 579), // Portugal, Iceland, Denmark
        (590, 590), (594, 594), (599, 599), // Poland, Romania, Hungary
        (640, 649), // Finland
        (700, 709), // Norway
        (730, 739), // Sweden
        (760, 769), // Switzerland & Liechtenstein
        (800, 849), // Italy, Spain
        (858, 860), // Slovakia, Czechia, Serbia
        (868, 879), // Türkiye, Netherlands
        (900, 919), // Austria
        (930, 949)  // Australia, New Zealand
    ];

    private static bool HasValidCheckDigit(string digits)
    {
        // GS1 check digit: weights 3 and 1 alternate from the rightmost data digit.
        var sum = 0;
        var weight = 3;
        for (var i = digits.Length - 2; i >= 0; i--, weight = 4 - weight)
            sum += (digits[i] - '0') * weight;

        return (10 - sum % 10) % 10 == digits[^1] - '0';
    }

    private static string? ExpandUpcE(string upcE)
    {
        if (upcE[0] is not ('0' or '1'))
            return null;

        var (system, d, check) = (upcE[0], upcE[1..7], upcE[7]);
        var body = d[5] switch
        {
            '0' or '1' or '2' => $"{d[..2]}{d[5]}0000{d[2..5]}",
            '3' => $"{d[..3]}00000{d[3..5]}",
            '4' => $"{d[..4]}00000{d[4]}",
            _ => $"{d[..5]}0000{d[5]}"
        };

        return $"{system}{body}{check}";
    }
}
