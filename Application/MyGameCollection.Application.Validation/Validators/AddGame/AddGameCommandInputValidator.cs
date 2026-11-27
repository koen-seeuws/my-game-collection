using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.AddGame;

public sealed class AddGameCommandInputValidator : CommandInputValidatorBase<AddGameCommand>
{
    public AddGameCommandInputValidator()
    {
        RuleFor(c => c.Title).RequiredText(CollectionItemLimits.MaxNameLength);
        RuleFor(c => c.Platform).RequiredText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Edition).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Genres).Genres();
        RuleFor(c => c.Developers).Companies("developer", "developers");
        RuleFor(c => c.Publishers).Companies("publisher", "publishers");
        RuleFor(c => c.Series).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.AgeRating).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Format).IsInEnum();
        RuleFor(c => c.Players).PlayerModes();
        RuleFor(c => c.Region).IsInEnum();
        RuleFor(c => c.Condition).IsInEnum();
        RuleFor(c => c.Completeness).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.ReleaseDate).ReleaseDate();
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.PurchaseOrigin).IsInEnum();
        RuleFor(c => c.Notes).OptionalText(CollectionItemLimits.MaxNotesLength);
        RuleFor(c => c.ThumbnailUrl).OptionalHttpUrl();
        RuleFor(c => c.CatalogId).CatalogId();
    }
}
