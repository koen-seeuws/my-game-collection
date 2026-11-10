using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Infrastructure.Persistence;

/// <summary>
/// Default platforms inserted into an empty database. Aliases include common abbreviations and the names
/// used by retailers / RAWG so barcode results can be matched to a platform automatically.
/// </summary>
internal static class PlatformSeed
{
    public static IEnumerable<Platform> Create() =>
    [
        // Nintendo
        new("NES", "Nintendo", 1983, ["Nintendo Entertainment System", "Famicom", "Family Computer"]),
        new("Super Nintendo", "Nintendo", 1990, ["SNES", "Super NES", "Super Nintendo Entertainment System", "Super Famicom", "SFC"]),
        new("Nintendo 64", "Nintendo", 1996, ["N64"]),
        new("GameCube", "Nintendo", 2001, ["Nintendo GameCube", "NGC", "GCN"]),
        new("Wii", "Nintendo", 2006, ["Nintendo Wii"]),
        new("Wii U", "Nintendo", 2012, ["Nintendo Wii U", "WiiU"]),
        new("Nintendo Switch", "Nintendo", 2017, ["NSW"]),
        new("Nintendo Switch 2", "Nintendo", 2025, ["Switch 2"]),
        new("Game Boy", "Nintendo", 1989, ["Nintendo Game Boy", "GB", "Gameboy"]),
        new("Game Boy Color", "Nintendo", 1998, ["GBC", "Gameboy Color", "Game Boy Colour"]),
        new("Game Boy Advance", "Nintendo", 2001, ["GBA", "Gameboy Advance", "Game Boy Advance SP"]),
        new("Nintendo DS", "Nintendo", 2004, ["NDS", "DS", "DSi", "DS Lite"]),
        new("Nintendo 3DS", "Nintendo", 2011, ["3DS", "2DS", "New Nintendo 3DS"]),
        new("Virtual Boy", "Nintendo", 1995, []),
        // Sega
        new("Master System", "Sega", 1986, ["Sega Master System", "SMS", "Mark III"]),
        new("Mega Drive", "Sega", 1988, ["Sega Mega Drive", "Genesis", "Sega Genesis", "Megadrive"]),
        new("Mega-CD", "Sega", 1991, ["Sega CD", "Mega CD", "Sega Mega-CD"]),
        new("32X", "Sega", 1994, ["Sega 32X", "Mega Drive 32X"]),
        new("Sega Saturn", "Sega", 1994, ["Saturn"]),
        new("Dreamcast", "Sega", 1998, ["Sega Dreamcast"]),
        new("Game Gear", "Sega", 1990, ["Sega Game Gear"]),
        // Sony
        new("PlayStation", "Sony", 1994, ["PS1", "PSX", "PSone", "PS one", "Sony PlayStation"]),
        new("PlayStation 2", "Sony", 2000, ["PS2", "Playstation2"]),
        new("PlayStation 3", "Sony", 2006, ["PS3", "Playstation3"]),
        new("PlayStation 4", "Sony", 2013, ["PS4", "Playstation4"]),
        new("PlayStation 5", "Sony", 2020, ["PS5", "Playstation5"]),
        new("PlayStation Portable", "Sony", 2004, ["PSP"]),
        new("PlayStation Vita", "Sony", 2011, ["PS Vita", "PSV", "Vita"]),
        // Microsoft
        new("Xbox", "Microsoft", 2001, ["Original Xbox", "Microsoft Xbox"]),
        new("Xbox 360", "Microsoft", 2005, ["X360"]),
        new("Xbox One", "Microsoft", 2013, ["XB1"]),
        new("Xbox Series X|S", "Microsoft", 2020, ["Xbox Series X", "Xbox Series S", "Xbox Series"]),
        // Atari
        new("Atari 2600", "Atari", 1977, ["VCS", "Atari VCS"]),
        new("Atari 7800", "Atari", 1986, []),
        new("Atari Jaguar", "Atari", 1993, ["Jaguar"]),
        new("Atari Lynx", "Atari", 1989, ["Lynx"]),
        // Others
        new("PC Engine", "NEC", 1987, ["TurboGrafx-16", "TurboGrafx 16", "TG16", "PC-Engine"]),
        new("Neo Geo AES", "SNK", 1990, ["Neo Geo", "NeoGeo", "AES"]),
        new("Neo Geo Pocket Color", "SNK", 1999, ["NGPC", "Neo Geo Pocket"]),
        new("3DO", "Panasonic", 1993, ["3DO Interactive Multiplayer"]),
        new("CD-i", "Philips", 1991, ["Philips CD-i", "CDi"]),
        new("Commodore 64", "Commodore", 1982, ["C64"]),
        new("Amiga", "Commodore", 1985, ["Commodore Amiga", "Amiga 500", "CD32", "Amiga CD32"]),
        new("PC", "Various", null, ["Windows", "MS-DOS", "DOS", "PC CD-ROM"]),
    ];
}
