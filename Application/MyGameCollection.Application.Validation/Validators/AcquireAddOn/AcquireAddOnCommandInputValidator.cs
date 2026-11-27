using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AcquireAddOn;

namespace MyGameCollection.Application.Validation.Validators.AcquireAddOn;

public sealed class AcquireAddOnCommandInputValidator : CommandInputValidatorBase<AcquireAddOnCommand>
{
    public AcquireAddOnCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.PurchaseDate).NotEmpty().WithMessage("Enter the purchase date.");
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.PurchaseOrigin).IsInEnum();
    }
}
