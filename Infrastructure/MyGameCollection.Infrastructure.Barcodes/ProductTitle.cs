using System.Text.RegularExpressions;

namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>
/// Turns a retail product title ("Halo 3 (Xbox 360) - PAL Video Game") into a game catalog search ("Halo 3").
/// </summary>
public static partial class ProductTitle
{
    public static string ToSearchTerm(string productName)
    {
        // "(Nintendo 64, 1996)", "[PAL]", "{UK}"
        var text = BracketedRegex().Replace(productName, " ");

        // Listings often append the platform or packaging as an extra " - " / " | " segment. The first segment
        // is the title; later ones are only kept if something besides noise remains.
        var segments = SegmentSeparatorRegex().Split(text);
        var parts = new List<string> { Clean(segments[0], isTitleSegment: true) };
        parts.AddRange(segments.Skip(1)
            .Select(segment => Clean(segment, isTitleSegment: false))
            .Where(segment => segment.Length > 0));

        var result = string.Join(" - ", parts.Where(p => p.Length > 0));
        return result.Length > 0 ? result : productName.Trim();
    }

    private static string Clean(string segment, bool isTitleSegment)
    {
        var text = ProductCodeRegex().Replace(NoiseRegex().Replace(segment, " "), " ");

        if (OnlyPlatformRegex().IsMatch(text))
            return string.Empty;

        // A bare platform name opening the title is usually part of it ("Wii Sports", "PlayStation All-Stars"),
        // but not when it comes with its manufacturer ("Nintendo Switch Mario Kart 8").
        text = isTitleSegment
            ? PlatformNotAtStartRegex().Replace(LeadingBrandedPlatformRegex().Replace(text, " "), " ")
            : PlatformRegex().Replace(ManufacturerRegex().Replace(text, " "), " ");

        text = WhitespaceRegex().Replace(text, " ").Trim();

        // Dangling words left over from "... Game for PlayStation".
        string previous;
        do
        {
            previous = text;
            text = TrailingFillerRegex().Replace(text, "").Trim().Trim(',', ';', ':', '-', '/', '|', '&').Trim();
        } while (text != previous);

        return text;
    }

    private const string Platforms =
        @"(?:(?:Nintendo|Sony|Sega|Microsoft|Atari|SNK|NEC)\s+)?" +
        @"(?:Nintendo\s*64|N64|Super\s+Nintendo(?:\s+Entertainment\s+System)?|SNES|Super\s+NES|" +
        @"Nintendo\s+Entertainment\s+System|NES|Nintendo\s+Switch(?:\s+2)?|Switch(?:\s+2)?|Game\s?Cube|NGC|Wii\s?U|Wii|" +
        @"Game\s?Boy(?:\s+(?:Advance(?:\s+SP)?|Color|Colour))?|GBA|GBC|GB|Nintendo\s+3DS|New\s+3DS|3DS|Nintendo\s+DS|NDS|DS|" +
        @"PlayStation\s*(?:[1-5]|One|Portable|Vita)?|PS\s?[1-5]|PSX|PSP|PS\s?Vita|Vita|" +
        @"Xbox\s*(?:360|One|Series\s+[XS](?:\s*\|\s*[XS])?)?|" +
        @"Genesis|Mega\s?Drive|Mega[\s-]?CD|Sega\s+CD|32X|Saturn|Dreamcast|Game\s+Gear|Master\s+System|" +
        @"Neo\s?Geo(?:\s+(?:AES|Pocket(?:\s+Colou?r)?))?|PC\s?Engine|TurboGrafx(?:-16)?|Jaguar|Lynx|" +
        @"Atari\s+2600|Atari\s+7800|3DO|CD-?i|WonderSwan|PC|Mac|Windows)";

    [GeneratedRegex(@"\([^)]*\)|\[[^\]]*\]|\{[^}]*\}")]
    private static partial Regex BracketedRegex();

    [GeneratedRegex(@"\s+[-–—|]\s+|\s*/\s*")]
    private static partial Regex SegmentSeparatorRegex();

    [GeneratedRegex(
        @"\b(?:video\s+games?|brand\s+new|factory\s+sealed|sealed|pre-?owned|used|complete\s+in\s+box|cib|boxed|" +
        @"pal|ntsc(?:[-\s]?(?:u/c|uc|u|j))?|region\s+free|import|uk|eu|euro|usa|us\s+version|" +
        @"cartridge|cart|disc|disk)\b", RegexOptions.IgnoreCase)]
    private static partial Regex NoiseRegex();

    // Catalogue numbers printed on Nintendo and Sony boxes: HAC-P-AAAAA, SLES-50330, CUSA-07408...
    [GeneratedRegex(@"\b(?:HAC|NUS|DOL|RVL|WUP|CTR|NTR|TWL|AGB|DMG|CGB|SLES|SLUS|SCES|SCUS|SLPS|SLPM|BLES|BLUS|BCES|BCUS|CUSA|PPSA)(?:-[A-Z0-9]+)*\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex ProductCodeRegex();

    [GeneratedRegex(@"^[\s,;:-]*" + Platforms + @"[\s,;:-]*$", RegexOptions.IgnoreCase)]
    private static partial Regex OnlyPlatformRegex();

    [GeneratedRegex(@"^\s*(?:Nintendo|Sony|Sega|Microsoft|Atari|SNK|NEC)\s+" + Platforms + @"(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex LeadingBrandedPlatformRegex();

    [GeneratedRegex(@"(?<=\S.*)\b" + Platforms + @"(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex PlatformNotAtStartRegex();

    [GeneratedRegex(@"\b" + Platforms + @"(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex PlatformRegex();

    [GeneratedRegex(@"\b(?:Nintendo|Sony|Sega|Microsoft|Atari|Edition|Game|Games)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ManufacturerRegex();

    [GeneratedRegex(@"\b(?:for|game|games|video|on|with)$", RegexOptions.IgnoreCase)]
    private static partial Regex TrailingFillerRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
