using MyGameCollection.Application.Abstractions.Commands.DeleteAddOn;

namespace MyGameCollection.Application.Validation.Validators.DeleteAddOn;

public sealed class DeleteAddOnCommandInputValidator : CommandInputValidatorBase<DeleteAddOnCommand>
{
    public DeleteAddOnCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
    }
}
