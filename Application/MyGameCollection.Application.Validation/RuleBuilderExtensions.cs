using FluentValidation;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation;

/// <summary>Rules shared by several commands, matching how the domain normalises (trims) and bounds the values.</summary>
public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> ruleBuilder, int maxLength) =>
        ruleBuilder
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .Must(value => value.Trim().Length <= maxLength)
            .WithMessage($"{{PropertyName}} cannot be longer than {maxLength} characters.");

    public static IRuleBuilderOptions<T, string?> OptionalText<T>(this IRuleBuilder<T, string?> ruleBuilder, int maxLength) =>
        ruleBuilder
            .Must(value => value is null || value.Trim().Length <= maxLength)
            .WithMessage($"{{PropertyName}} cannot be longer than {maxLength} characters.");

    /// <summary>Empty, or an absolute http(s) URL, e.g. of a thumbnail image.</summary>
    public static IRuleBuilderOptions<T, string?> OptionalHttpUrl<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .OptionalText(CollectionItemLimits.MaxUrlLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || IsHttpUrl(value.Trim()))
            .WithMessage("{PropertyName} must be an absolute http(s) URL.");

    /// <summary>At most <see cref="CollectionItemLimits.MaxGenres"/> genres, none of them too long.</summary>
    public static IRuleBuilderOptions<T, IReadOnlyList<string>> Genres<T>(this IRuleBuilder<T, IReadOnlyList<string>> ruleBuilder) =>
        ruleBuilder.Names(CollectionItemLimits.MaxGenres, "genre", "genres");

    /// <summary>At most <see cref="CollectionItemLimits.MaxCompanies"/> developers or publishers, none of them too long.</summary>
    public static IRuleBuilderOptions<T, IReadOnlyList<string>> Companies<T>(this IRuleBuilder<T, IReadOnlyList<string>> ruleBuilder,
        string singular, string plural) =>
        ruleBuilder.Names(CollectionItemLimits.MaxCompanies, singular, plural);

    /// <summary>At most <paramref name="maxCount"/> names (blanks aside), none of them too long.</summary>
    private static IRuleBuilderOptions<T, IReadOnlyList<string>> Names<T>(this IRuleBuilder<T, IReadOnlyList<string>> ruleBuilder,
        int maxCount, string singular, string plural) =>
        ruleBuilder
            .Must(names => names is null || names.Count(n => !string.IsNullOrWhiteSpace(n)) <= maxCount)
            .WithMessage($"A game cannot have more than {maxCount} {plural}.")
            .Must(names => names is null || names.All(n => n is null || n.Trim().Length <= CollectionItemLimits.MaxShortTextLength))
            .WithMessage($"A {singular} cannot be longer than {CollectionItemLimits.MaxShortTextLength} characters.");

    /// <summary>Empty, or a year, month or day that exists (see <see cref="Core.Domain.Abstractions.ReleaseDate.Problem"/>).</summary>
    public static IRuleBuilderOptions<T, ReleaseDate?> ReleaseDate<T>(this IRuleBuilder<T, ReleaseDate?> ruleBuilder) =>
        ruleBuilder
            .Must(value => value?.Problem is null)
            .WithMessage((_, value) => value?.Problem ?? string.Empty);

    /// <summary>Empty, or player counts within bounds (see <see cref="Core.Domain.Abstractions.PlayerModes.Problem"/>).</summary>
    public static IRuleBuilderOptions<T, PlayerModes?> PlayerModes<T>(this IRuleBuilder<T, PlayerModes?> ruleBuilder) =>
        ruleBuilder
            .Must(value => value?.Problem is null)
            .WithMessage((_, value) => value?.Problem ?? string.Empty);

    public static IRuleBuilderOptions<T, decimal?> Price<T>(this IRuleBuilder<T, decimal?> ruleBuilder) =>
        ruleBuilder
            .GreaterThanOrEqualTo(0m)
            .WithMessage("A price cannot be negative.");

    /// <summary>Empty, or an id in the external catalog (IGDB ids are positive).</summary>
    public static IRuleBuilderOptions<T, long?> CatalogId<T>(this IRuleBuilder<T, long?> ruleBuilder) =>
        ruleBuilder
            .GreaterThan(0L)
            .WithMessage("{PropertyName} must be positive.");

    public static IRuleBuilderOptions<T, Guid> ItemKey<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Choose an item.");

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
