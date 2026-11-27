using System.Globalization;
using static MyGameCollection.Core.Domain.Abstractions.CollectionItemLimits;

namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>
/// When a game or console came out, as precisely as it is known: a year, a month of a year, or a full date.
/// Catalogs such as IGDB often only know the year of older or obscure titles.
/// </summary>
/// <remarks>
/// Not validated on construction so forms can hold what the user typed; see <see cref="Problem"/>.
/// Ordered chronologically, with a year before its months and a month before its days.
/// </remarks>
public readonly record struct ReleaseDate(int Year, int? Month = null, int? Day = null) : IComparable<ReleaseDate>
{
    public static ReleaseDate FromDate(DateOnly date) => new(date.Year, date.Month, date.Day);

    /// <summary>Why this is not a valid release date, or <c>null</c> when it is.</summary>
    public string? Problem =>
        Year is < MinReleaseYear or > MaxReleaseYear ? $"The release year must be between {MinReleaseYear} and {MaxReleaseYear}."
        : Month is < 1 or > 12 ? "The release month must be between 1 and 12."
        : Day is not null && Month is null ? "Enter the release month as well as the day."
        : Day is < 1 || Day > DateTime.DaysInMonth(Year, Month ?? 1) ? "That day does not exist in the release month."
        : null;

    public int CompareTo(ReleaseDate other) =>
        (Year, Month ?? 0, Day ?? 0).CompareTo((other.Year, other.Month ?? 0, other.Day ?? 0));

    /// <summary>"1991", "Jun 1991" or "23 Jun 1991", in the current culture.</summary>
    public override string ToString() => (Month, Day) switch
    {
        ({ } month, { } day) => new DateOnly(Year, month, day).ToString("d MMM yyyy", CultureInfo.CurrentCulture),
        ({ } month, null) => new DateOnly(Year, month, 1).ToString("MMM yyyy", CultureInfo.CurrentCulture),
        _ => Year.ToString(CultureInfo.InvariantCulture)
    };
}
