using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.UpdateAddOn;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.UpdateAddOn;

public sealed class UpdateAddOnCommandInputValidator : CommandInputValidatorBase<UpdateAddOnCommand>
{
    public UpdateAddOnCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.Title).RequiredText(CollectionItemLimits.MaxNameLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.Format).IsInEnum();
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
