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
        RuleFor(c => c.Region).IsInEnum();
        RuleFor(c => c.Condition).IsInEnum();
        RuleFor(c => c.Completeness).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.ReleaseYear).ReleaseYear();
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.Notes).OptionalText(CollectionItemLimits.MaxNotesLength);
        RuleFor(c => c.ThumbnailUrl).OptionalHttpUrl();
    }
}
