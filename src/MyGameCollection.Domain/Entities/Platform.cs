using MyGameCollection.Domain.Common;

namespace MyGameCollection.Domain.Entities;

/// <summary>
/// A gaming platform (e.g. "Super Nintendo", "Mega Drive"). Both games and consoles belong to a platform.
/// </summary>
public class Platform : BaseEntity
{
    public Platform(string name, string manufacturer, int? releaseYear = null, IEnumerable<string>? aliases = null)
        : this(Guid.CreateVersion7(), name, manufacturer, releaseYear, aliases) { }

    /// <summary>Rebuilds an existing platform (used by persistence).</summary>
    public Platform(Guid id, string name, string manufacturer, int? releaseYear, IEnumerable<string>? aliases) : base(id)
    {
        Rename(name);
        Manufacturer = manufacturer.Trim();
        ReleaseYear = releaseYear;
        SetAliases(aliases ?? []);
    }

    public string Name { get; private set; } = string.Empty;
    public string Manufacturer { get; set; }
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Alternative names used to recognise the platform in product titles from barcode lookups
    /// (e.g. "SNES", "Super Famicom").
    /// </summary>
    public IReadOnlyList<string> Aliases { get; private set; } = [];

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Platform name is required.", nameof(name));
        Name = name.Trim();
    }

    public void SetAliases(IEnumerable<string> aliases) =>
        Aliases = aliases
            .Select(a => a.Trim())
            .Where(a => a.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>All names (primary + aliases) that identify this platform.</summary>
    public IEnumerable<string> AllNames() => Aliases.Prepend(Name);
}
