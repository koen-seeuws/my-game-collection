using System.Text.RegularExpressions;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Features.Barcodes.Common;

public sealed record PlatformName(Guid PlatformId, string Name);

public sealed record ParsedProduct(string Title, Guid? PlatformId, ItemType Type, Region? Region);

/// <summary>
/// Turns noisy retail product titles like "Super Mario World (SNES) - PAL Version - Video Game"
/// into a clean title, a recognised platform, an item type and (when mentioned) a region.
/// </summary>
public static partial class ProductTitleParser
{
    public static ParsedProduct Parse(string rawTitle, string? category, IReadOnlyCollection<PlatformName> platforms)
    {
        var haystack = $"{rawTitle} {category}";

        var platformMatch = platforms
            .Select(p => (p.PlatformId, p.Name, Match: WholeWord(p.Name).Match(haystack)))
            .Where(x => x.Match.Success)
            .OrderByDescending(x => x.Name.Length) // "Game Boy Advance" beats "Game Boy"
            .FirstOrDefault();

        // Strip platform names first: "Master System" or "Entertainment System" must not flag a game as hardware.
        var withoutPlatforms = platforms.OrderByDescending(p => p.Name.Length)
            .Aggregate(haystack, (text, p) => WholeWord(p.Name).Replace(text, " "));
        var type = ConsoleKeywords().IsMatch(withoutPlatforms) ? ItemType.Console : ItemType.Game;
        var region = DetectRegion(haystack);

        var title = CleanTitle(rawTitle, platforms.Select(p => p.Name));
        if (title.Length == 0) title = rawTitle.Trim();

        return new ParsedProduct(title, platformMatch.Match is null ? null : platformMatch.PlatformId, type, region);
    }

    internal static string CleanTitle(string rawTitle, IEnumerable<string> platformNames)
    {
        var title = rawTitle;

        // Drop bracketed segments that only carry noise: "(PAL)", "[Nintendo 64]", "(Renewed)".
        title = Brackets().Replace(title, m => IsNoise(m.Groups["inner"].Value, platformNames) ? " " : m.Value);

        // Remove platform names that are used as a suffix/prefix ("... - Nintendo 64", "PS2 ...").
        foreach (var name in platformNames.OrderByDescending(n => n.Length))
            title = WholeWord(name).Replace(title, " ");

        title = NoiseWords().Replace(title, " ");
        title = Regex.Replace(title, @"\s+", " ");
        title = Regex.Replace(title, @"(\s*[-–|,:/]\s*)+$", "");   // trailing separators
        title = Regex.Replace(title, @"^(\s*[-–|,:/]\s*)+", "");   // leading separators
        title = Regex.Replace(title, @"\s+([-–|,:/])\s+([-–|,:/])\s+", " $2 ");
        title = Regex.Replace(title, @"\(\s*\)|\[\s*\]", "");
        return title.Trim(' ', '-', '–', '|', ',', ':', '/');
    }

    private static bool IsNoise(string inner, IEnumerable<string> platformNames)
    {
        var remaining = inner;
        foreach (var name in platformNames.OrderByDescending(n => n.Length))
            remaining = WholeWord(name).Replace(remaining, " ");
        remaining = NoiseWords().Replace(remaining, " ");
        remaining = RegionWords().Replace(remaining, " ");
        remaining = Regex.Replace(remaining, @"[\s,/\-–|&]+", "");
        return remaining.Length == 0 || Regex.IsMatch(remaining, @"^\d{4}$"); // only noise or a year
    }

    private static Region? DetectRegion(string text)
    {
        // Short codes are matched case-sensitively so "Among Us" or "Rated E" don't count as a region.
        if (Regex.IsMatch(text, @"\bNTSC[\s-]?J\b|\bJPN?\b") || Regex.IsMatch(text, @"\bJapan(ese)?\b", RegexOptions.IgnoreCase)) return Region.NtscJ;
        if (Regex.IsMatch(text, @"\bPAL\b|\bEUR?\b|\bUK\b") || Regex.IsMatch(text, @"\bEurope(an)?\b", RegexOptions.IgnoreCase)) return Region.Pal;
        if (Regex.IsMatch(text, @"\bNTSC([\s-]?US?)?\b|\bUSA?\b") || Regex.IsMatch(text, @"\bNorth America\b", RegexOptions.IgnoreCase)) return Region.NtscU;
        if (Regex.IsMatch(text, @"\bRegion[\s-]?Free\b", RegexOptions.IgnoreCase)) return Region.RegionFree;
        return null;
    }

    private static Regex WholeWord(string phrase) =>
        new($@"(?<![\w]){Regex.Escape(phrase).Replace(@"\ ", @"[\s\-]?")}(?![\w])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [GeneratedRegex(@"[\(\[](?<inner>[^\)\]]*)[\)\]]")]
    private static partial Regex Brackets();

    [GeneratedRegex(@"\b(video\s?games?|videogames?|game\s+for|for|renewed|refurbished|import|version|edition\s+only|brand\s+new|new|sealed|used|pre[\s-]?owned|platinum|classics|players'?\s+choice|greatest\s+hits|nintendo\s+selects)\b(?=\s*[\)\]\-–|,]|\s*$)",
        RegexOptions.IgnoreCase)]
    private static partial Regex NoiseWords();

    [GeneratedRegex(@"\b(PAL|NTSC(-?[UJ])?|UK|EU|EUR|USA?|JPN?|Japan(ese)?|Europe(an)?|Region\s?Free|English|multi(language)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegionWords();

    [GeneratedRegex(@"\b(consoles?|system|hardware|controller\s+included)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ConsoleKeywords();
}
