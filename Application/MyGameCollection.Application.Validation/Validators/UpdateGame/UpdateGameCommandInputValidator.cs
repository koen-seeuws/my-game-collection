using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.UpdateGame;

public sealed class UpdateGameCommandInputValidator : CommandInputValidatorBase<UpdateGameCommand>
{
    public UpdateGameCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.Title).RequiredText(CollectionItemLimits.MaxNameLength);
        RuleFor(c => c.Platform).RequiredText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Edition).OptionalText(CollectionItemLimits.MaxShortTextLength);
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
