using System.Text.RegularExpressions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Well-known (retro) systems, offered as suggestions when entering games and consoles.</summary>
public static class KnownPlatforms
{
    /// <param name="Aliases">Other names for the system, such as IGDB's naming and common abbreviations.</param>
    public sealed record Platform(string Name, string Manufacturer, params string[] Aliases);

    public static IReadOnlyList<Platform> All { get; } =
    [
        new("Atari 2600", "Atari", "Atari VCS"),
        new("Atari 7800", "Atari"),
        new("Atari Jaguar", "Atari", "Jaguar"),
        new("Atari Lynx", "Atari", "Lynx"),
        new("ColecoVision", "Coleco"),
        new("Intellivision", "Mattel"),
        new("Commodore 64", "Commodore", "Commodore C64/128/MAX", "C64"),
        new("Amiga", "Commodore", "Commodore Amiga"),
        new("ZX Spectrum", "Sinclair", "Sinclair ZX Spectrum"),
        new("MSX", "Various"),
        new("NES", "Nintendo", "Nintendo Entertainment System", "Famicom", "Family Computer"),
        new("Super Nintendo", "Nintendo", "Super Nintendo Entertainment System", "SNES", "Super NES", "Super Famicom"),
        new("Nintendo 64", "Nintendo", "N64"),
        new("GameCube", "Nintendo", "Nintendo GameCube", "Game Cube", "NGC"),
        new("Wii", "Nintendo", "Nintendo Wii"),
        new("Wii U", "Nintendo", "Nintendo Wii U", "WiiU"),
        new("Nintendo Switch", "Nintendo", "Switch"),
        new("Game Boy", "Nintendo", "GB"),
        new("Game Boy Color", "Nintendo", "Game Boy Colour", "GBC"),
        new("Game Boy Advance", "Nintendo", "GBA"),
        new("Nintendo DS", "Nintendo", "NDS", "DS"),
        new("Nintendo 3DS", "Nintendo", "3DS"),
        new("Virtual Boy", "Nintendo"),
        new("Master System", "Sega", "Sega Master System/Mark III", "Sega Master System", "SMS"),
        new("Mega Drive", "Sega", "Sega Mega Drive/Genesis", "Sega Mega Drive", "Sega Genesis", "Genesis",
            "Megadrive", "Genesis/MegaDrive"),
        new("Mega-CD", "Sega", "Sega CD", "Mega CD", "Sega Mega-CD"),
        new("32X", "Sega", "Sega 32X"),
        new("Saturn", "Sega", "Sega Saturn"),
        new("Dreamcast", "Sega", "Sega Dreamcast", "DC"),
        new("Game Gear", "Sega", "Sega Game Gear"),
        new("PlayStation", "Sony", "Sony PlayStation", "PS1", "PSX", "PS One"),
        new("PlayStation 2", "Sony", "Sony PlayStation 2", "PS2"),
        new("PlayStation 3", "Sony", "Sony PlayStation 3", "PS3"),
        new("PlayStation 4", "Sony", "Sony PlayStation 4", "PS4"),
        new("PlayStation Portable", "Sony", "PSP"),
        new("PlayStation Vita", "Sony", "PS Vita", "PSVita", "Vita"),
        new("Xbox", "Microsoft", "Microsoft Xbox"),
        new("Xbox 360", "Microsoft", "Microsoft Xbox 360", "X360"),
        new("Neo Geo AES", "SNK", "NeoGeo AES"),
        new("Neo Geo Pocket Color", "SNK", "Neo Geo Pocket Colour", "NGPC"),
        new("PC Engine", "NEC", "TurboGrafx-16/PC Engine", "TurboGrafx-16", "TurboGrafx", "TG16"),
        new("3DO", "Panasonic", "3DO Interactive Multiplayer"),
        new("Philips CD-i", "Philips", "CD-i", "CDi"),
        new("WonderSwan", "Bandai")
    ];

    public static IEnumerable<string> Manufacturers =>
        All.Select(p => p.Manufacturer).Distinct().Order();

    public static string? ManufacturerOf(string platformName) => Resolve(platformName)?.Manufacturer;

    /// <summary>The well-known system with this name or alias, if any.</summary>
    public static Platform? Resolve(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : All.FirstOrDefault(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                                      || p.Aliases.Any(a => a.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Translates a catalog's platform naming ("Sega Mega Drive/Genesis") into the one used in this collection:
    /// a name from <paramref name="preferred"/> for the same system, else the well-known name, else the catalog's own.
    /// </summary>
    public static string Match(IEnumerable<string> preferred, string name, params string?[] otherNames)
    {
        var preferredNames = preferred.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        var candidates = otherNames.Prepend(name).OfType<string>().Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

        if (candidates.Select(c => preferredNames.FirstOrDefault(p => p.Equals(c, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(p => p is not null) is { } exact)
            return exact;

        if (candidates.Select(Resolve).FirstOrDefault(p => p is not null) is not { } known)
            return name;

        return preferredNames.FirstOrDefault(p => Resolve(p) == known) ?? known.Name;
    }

    /// <summary>
    /// The system mentioned in free text such as a product title ("Halo 3 (Xbox 360)"), in this collection's naming.
    /// The longest mention wins, so "PlayStation 2" beats "PlayStation".
    /// </summary>
    public static string? DetectIn(string? text, IEnumerable<string> preferred)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var preferredNames = preferred.ToList();
        var mention = preferredNames
            .Concat(All.SelectMany(p => p.Aliases.Prepend(p.Name)))
            .Where(n => n.Length >= 2)
            .Where(n => Regex.IsMatch(text, $@"(?<![\w-]){Regex.Escape(n)}(?![\w-])", RegexOptions.IgnoreCase))
            .MaxBy(n => n.Length);

        return mention is null ? null : Match(preferredNames, mention);
    }

    /// <summary>Suggestions matching <paramref name="search"/>: the given names first, then the known platforms.</summary>
    public static IEnumerable<string> Suggest(IEnumerable<string> preferred, IEnumerable<string> known, string? search) =>
        preferred.Concat(known)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => string.IsNullOrWhiteSpace(search) || name.Contains(search, StringComparison.OrdinalIgnoreCase));
}
