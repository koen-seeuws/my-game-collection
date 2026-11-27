using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.UpdateConsole;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.UpdateConsole;

public sealed class UpdateConsoleCommandInputValidator : CommandInputValidatorBase<UpdateConsoleCommand>
{
    public UpdateConsoleCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.Name).RequiredText(CollectionItemLimits.MaxNameLength);
        RuleFor(c => c.Manufacturer).RequiredText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Model).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Colour).OptionalText(CollectionItemLimits.MaxShortTextLength);
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
