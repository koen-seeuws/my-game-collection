using MyGameCollection.Application.Abstractions.Commands.DeleteGame;

namespace MyGameCollection.Application.Validation.Validators.DeleteGame;

public sealed class DeleteGameCommandInputValidator : CommandInputValidatorBase<DeleteGameCommand>
{
    public DeleteGameCommandInputValidator()
    {
        RuleFor(c => c.Key).ItemKey();
    }
}
