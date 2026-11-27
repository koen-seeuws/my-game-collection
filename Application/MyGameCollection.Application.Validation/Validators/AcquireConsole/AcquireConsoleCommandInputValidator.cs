using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.AcquireConsole;

namespace MyGameCollection.Application.Validation.Validators.AcquireConsole;

public sealed class AcquireConsoleCommandInputValidator : CommandInputValidatorBase<AcquireConsoleCommand>
{
    public AcquireConsoleCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
        RuleFor(c => c.PurchaseDate).NotEmpty().WithMessage("Enter the purchase date.");
        RuleFor(c => c.PurchasePrice).Price();
        RuleFor(c => c.PurchaseOrigin).IsInEnum();
    }
}
