using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;

namespace MyGameCollection.Application.Validation.Validators.AcquireGame;

public sealed class AcquireGameCommandInputValidator : CommandInputValidatorBase<AcquireGameCommand>
{
    public AcquireGameCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.PurchaseDate).NotEmpty().WithMessage("Enter the purchase date.");
        RuleFor(c => c.PurchasePrice).Price();
    }
}
