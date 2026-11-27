using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AddAddOn;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Application.Validation.Validators.AddAddOn;

public sealed class AddAddOnCommandInputValidator : CommandInputValidatorBase<AddAddOnCommand>
{
    public AddAddOnCommandInputValidator()
    {
        RuleFor(c => c.GameKey).ItemKey();
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
