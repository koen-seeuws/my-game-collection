using MyGameCollection.Application.Abstractions.Commands.DeleteAccessory;

namespace MyGameCollection.Application.Validation.Validators.DeleteAccessory;

public sealed class DeleteAccessoryCommandInputValidator : CommandInputValidatorBase<DeleteAccessoryCommand>
{
    public DeleteAccessoryCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
    }
}
