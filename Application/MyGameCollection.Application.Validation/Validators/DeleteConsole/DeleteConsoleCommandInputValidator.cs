using MyGameCollection.Application.Abstractions.Commands.DeleteConsole;

namespace MyGameCollection.Application.Validation.Validators.DeleteConsole;

public sealed class DeleteConsoleCommandInputValidator : CommandInputValidatorBase<DeleteConsoleCommand>
{
    public DeleteConsoleCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
    }
}
