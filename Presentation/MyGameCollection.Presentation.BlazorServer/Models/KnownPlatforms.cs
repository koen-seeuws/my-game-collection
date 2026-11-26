namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Well-known (retro) systems, offered as suggestions when entering games and consoles.</summary>
public static class KnownPlatforms
{
    public sealed record Platform(string Name, string Manufacturer);

    public static IReadOnlyList<Platform> All { get; } =
    [
        new("Atari 2600", "Atari"),
        new("Atari 7800", "Atari"),
        new("Atari Jaguar", "Atari"),
        new("Atari Lynx", "Atari"),
        new("ColecoVision", "Coleco"),
        new("Intellivision", "Mattel"),
        new("Commodore 64", "Commodore"),
        new("Amiga", "Commodore"),
        new("ZX Spectrum", "Sinclair"),
        new("MSX", "Various"),
        new("NES", "Nintendo"),
        new("Super Nintendo", "Nintendo"),
        new("Nintendo 64", "Nintendo"),
        new("GameCube", "Nintendo"),
        new("Wii", "Nintendo"),
        new("Wii U", "Nintendo"),
        new("Nintendo Switch", "Nintendo"),
        new("Game Boy", "Nintendo"),
        new("Game Boy Color", "Nintendo"),
        new("Game Boy Advance", "Nintendo"),
        new("Nintendo DS", "Nintendo"),
        new("Nintendo 3DS", "Nintendo"),
        new("Virtual Boy", "Nintendo"),
        new("Master System", "Sega"),
        new("Mega Drive", "Sega"),
        new("Mega-CD", "Sega"),
        new("32X", "Sega"),
        new("Saturn", "Sega"),
        new("Dreamcast", "Sega"),
        new("Game Gear", "Sega"),
        new("PlayStation", "Sony"),
        new("PlayStation 2", "Sony"),
        new("PlayStation 3", "Sony"),
        new("PlayStation 4", "Sony"),
        new("PlayStation Portable", "Sony"),
        new("PlayStation Vita", "Sony"),
        new("Xbox", "Microsoft"),
        new("Xbox 360", "Microsoft"),
        new("Neo Geo AES", "SNK"),
        new("Neo Geo Pocket Color", "SNK"),
        new("PC Engine", "NEC"),
        new("3DO", "Panasonic"),
        new("Philips CD-i", "Philips"),
        new("WonderSwan", "Bandai")
    ];

    public static IEnumerable<string> Manufacturers =>
        All.Select(p => p.Manufacturer).Distinct().Order();

    public static string? ManufacturerOf(string platformName) =>
        All.FirstOrDefault(p => string.Equals(p.Name, platformName, StringComparison.OrdinalIgnoreCase))?.Manufacturer;

    /// <summary>Suggestions matching <paramref name="search"/>: the given names first, then the known platforms.</summary>
    public static IEnumerable<string> Suggest(IEnumerable<string> preferred, IEnumerable<string> known, string? search) =>
        preferred.Concat(known)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(name => string.IsNullOrWhiteSpace(search) || name.Contains(search, StringComparison.OrdinalIgnoreCase));
}
