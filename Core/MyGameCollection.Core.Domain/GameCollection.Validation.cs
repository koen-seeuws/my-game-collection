using MyGameCollection.Core.Domain.Abstractions;
using static MyGameCollection.Core.Domain.Abstractions.CollectionItemLimits;

namespace MyGameCollection.Core.Domain;

/// <summary>Validates and normalises items before they enter the collection.</summary>
internal static class GameCollectionValidation
{
    public static Game Validated(Game game)
    {
        var normalised = game with
        {
            Title = Required(game.Title, "title", MaxNameLength),
            Platform = Required(game.Platform, "platform", MaxShortTextLength),
            Edition = Optional(game.Edition, "edition", MaxShortTextLength),
            Genres = Names(game.Genres, "genres", MaxGenres),
            Developers = Names(game.Developers, "developers", MaxCompanies),
            Publishers = Names(game.Publishers, "publishers", MaxCompanies),
            Series = Optional(game.Series, "series", MaxShortTextLength),
            AgeRating = Optional(game.AgeRating, "ageRating", MaxShortTextLength),
            Notes = Optional(game.Notes, "notes", MaxNotesLength),
            ThumbnailUrl = OptionalUrl(game.ThumbnailUrl, "thumbnailUrl"),
            Players = game.Players?.Normalised(),
            // A download is always new, and an item on the wishlist has not been bought yet.
            PurchaseOrigin = game.Status == CollectionStatus.Owned && game.Format == GameFormat.Physical
                ? game.PurchaseOrigin
                : null
        };

        EnsureValid(normalised.ReleaseDate, normalised.PurchasePrice);
        EnsureValidCatalogId(normalised.CatalogId);
        if (normalised.Type is { } type && !Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException("type", type, "Unknown game type.");
        if (normalised.Players?.Problem is { } problem)
            throw new ArgumentOutOfRangeException("players", normalised.Players, problem);

        return normalised;
    }

    public static AddOn Validated(AddOn addOn)
    {
        var normalised = addOn with
        {
            Title = Required(addOn.Title, "title", MaxNameLength),
            Notes = Optional(addOn.Notes, "notes", MaxNotesLength),
            ThumbnailUrl = OptionalUrl(addOn.ThumbnailUrl, "thumbnailUrl"),
            // A download is always new, and an item on the wishlist has not been bought yet.
            PurchaseOrigin = addOn.Status == CollectionStatus.Owned && addOn.Format == GameFormat.Physical
                ? addOn.PurchaseOrigin
                : null
        };

        EnsureValid(normalised.ReleaseDate, normalised.PurchasePrice);
        EnsureValidCatalogId(normalised.CatalogId);
        if (!Enum.IsDefined(normalised.Type))
            throw new ArgumentOutOfRangeException("type", normalised.Type, "Unknown add-on type.");

        return normalised;
    }

    public static GameConsole Validated(GameConsole console)
    {
        var normalised = console with
        {
            Name = Required(console.Name, "name", MaxNameLength),
            Manufacturer = Required(console.Manufacturer, "manufacturer", MaxShortTextLength),
            Model = Optional(console.Model, "model", MaxShortTextLength),
            Colour = Optional(console.Colour, "colour", MaxShortTextLength),
            Notes = Optional(console.Notes, "notes", MaxNotesLength),
            ThumbnailUrl = OptionalUrl(console.ThumbnailUrl, "thumbnailUrl"),
            // An item on the wishlist has not been bought yet.
            PurchaseOrigin = console.Status == CollectionStatus.Owned ? console.PurchaseOrigin : null
        };

        EnsureValid(normalised.ReleaseDate, normalised.PurchasePrice);
        EnsureValidCatalogId(normalised.CatalogId);
        return normalised;
    }

    public static void EnsureValidPrice(decimal? price, string paramName)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(paramName, price, "A price cannot be negative.");
    }

    private static void EnsureValidCatalogId(long? catalogId)
    {
        if (catalogId <= 0)
            throw new ArgumentOutOfRangeException(nameof(catalogId), catalogId, "A catalog id must be positive.");
    }

    private static void EnsureValid(ReleaseDate? releaseDate, decimal? purchasePrice)
    {
        if (releaseDate?.Problem is { } problem)
            throw new ArgumentOutOfRangeException(nameof(releaseDate), releaseDate, problem);

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

    /// <summary>
    /// A list of names (genres, developers...) trimmed, without blanks or duplicates (ignoring case), in their
    /// original order.
    /// </summary>
    private static IReadOnlyList<string> Names(IReadOnlyList<string>? values, string name, int maxCount)
    {
        var normalised = (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Required(value, name, MaxShortTextLength))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalised.Count > maxCount)
            throw new ArgumentException($"A game cannot have more than {maxCount} {name}.", name);

        return normalised;
    }

    /// <summary>An optional absolute http(s) URL, e.g. of a thumbnail image.</summary>
    private static string? OptionalUrl(string? value, string name)
    {
        var url = Optional(value, name, MaxUrlLength);

        if (url is not null && !(Uri.TryCreate(url, UriKind.Absolute, out var uri)
                                 && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
            throw new ArgumentException($"{name} must be an absolute http(s) URL.", name);

        return url;
    }
}
