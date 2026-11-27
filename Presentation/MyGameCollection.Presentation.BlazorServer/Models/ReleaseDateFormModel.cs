using System.ComponentModel.DataAnnotations;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>
/// The release date fields of the game and console forms: a year, optionally with a month, optionally with a day,
/// so a release only known by its year can still be entered.
/// </summary>
public abstract class ReleaseDateFormModel : IValidatableObject
{
    [Range(CollectionItemLimits.MinReleaseYear, CollectionItemLimits.MaxReleaseYear)]
    public int? ReleaseYear { get; set; }

    [Range(1, 12)]
    public int? ReleaseMonth { get; set; }

    [Range(1, 31)]
    public int? ReleaseDay { get; set; }

    /// <summary>The fields as one release date; empty without a year.</summary>
    public ReleaseDate? ReleaseDate
    {
        get => ReleaseYear is { } year ? new ReleaseDate(year, ReleaseMonth, ReleaseDay) : null;
        set => (ReleaseYear, ReleaseMonth, ReleaseDay) = (value?.Year, value?.Month, value?.Day);
    }

    /// <summary>Checks the fields together: a month or day needs a year, and the day has to exist.</summary>
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReleaseYear is null && (ReleaseMonth is not null || ReleaseDay is not null))
            yield return new ValidationResult("Enter the release year as well.", [nameof(ReleaseYear)]);
        else if (ReleaseDate?.Problem is { } problem && ReleaseYear is >= CollectionItemLimits.MinReleaseYear and <= CollectionItemLimits.MaxReleaseYear)
            yield return new ValidationResult(problem, [ReleaseMonth is null ? nameof(ReleaseMonth) : nameof(ReleaseDay)]);
    }
}
