namespace MyGameCollection.Core.Domain;

/// <summary>Validates and normalises item details before they enter the collection.</summary>
internal static class GameCollectionValidation
{
    public const int MaxNameLength = 200;
    public const int MaxShortTextLength = 100;
    public const int MaxNotesLength = 2000;
    public const int MinReleaseYear = 1950;
    public const int MaxReleaseYear = 2100;

    public static GameDetails Validated(GameDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        var normalised = details with
        {
            Title = Required(details.Title, nameof(details.Title), MaxNameLength),
            Platform = Required(details.Platform, nameof(details.Platform), MaxShortTextLength),
            Notes = Optional(details.Notes, nameof(details.Notes), MaxNotesLength)
        };

        EnsureValid(normalised.ReleaseYear, normalised.PurchasePrice);
        return normalised;
    }

    public static ConsoleDetails Validated(ConsoleDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        var normalised = details with
        {
            Name = Required(details.Name, nameof(details.Name), MaxNameLength),
            Manufacturer = Required(details.Manufacturer, nameof(details.Manufacturer), MaxShortTextLength),
            Model = Optional(details.Model, nameof(details.Model), MaxShortTextLength),
            Notes = Optional(details.Notes, nameof(details.Notes), MaxNotesLength)
        };

        EnsureValid(normalised.ReleaseYear, normalised.PurchasePrice);
        return normalised;
    }

    public static void EnsureValidPrice(decimal? price, string paramName)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(paramName, price, "A price cannot be negative.");
    }

    private static void EnsureValid(int? releaseYear, decimal? purchasePrice)
    {
        if (releaseYear is < MinReleaseYear or > MaxReleaseYear)
            throw new ArgumentOutOfRangeException(nameof(releaseYear), releaseYear,
                $"The release year must be between {MinReleaseYear} and {MaxReleaseYear}.");

        EnsureValidPrice(purchasePrice, nameof(purchasePrice));
    }

    private static string Required(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} is required.", name);

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{name} cannot be longer than {maxLength} characters.", name);

        return trimmed;
    }

    private static string? Optional(string? value, string name, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, name, maxLength);
}
