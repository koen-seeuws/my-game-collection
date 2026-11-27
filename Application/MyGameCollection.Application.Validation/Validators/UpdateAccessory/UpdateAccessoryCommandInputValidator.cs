using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.UpdateAccessory;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.UpdateAccessory;

public sealed class UpdateAccessoryCommandInputValidator : CommandInputValidatorBase<UpdateAccessoryCommand>
{
    public UpdateAccessoryCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.Name).RequiredText(CollectionItemLimits.MaxNameLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.Platform).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Manufacturer).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Model).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Colour).OptionalText(CollectionItemLimits.MaxShortTextLength);
        RuleFor(c => c.Quantity).AccessoryQuantity();
        RuleFor(c => c.Region).IsInEnum();
        RuleFor(c => c.Condition).IsInEnum();
        RuleFor(c => c.Completeness).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.PurchaseOrigin).IsInEnum();
        RuleFor(c => c.Notes).OptionalText(CollectionItemLimits.MaxNotesLength);
        RuleFor(c => c.ThumbnailUrl).OptionalHttpUrl();
    }
}
