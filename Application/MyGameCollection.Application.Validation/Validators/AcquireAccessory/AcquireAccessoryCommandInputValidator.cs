using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AcquireAccessory;

namespace MyGameCollection.Application.Validation.Validators.AcquireAccessory;

public sealed class AcquireAccessoryCommandInputValidator : CommandInputValidatorBase<AcquireAccessoryCommand>
{
    public AcquireAccessoryCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.PurchaseDate).NotEmpty().WithMessage("Enter the purchase date.");
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.PurchaseOrigin).IsInEnum();
    }
}
