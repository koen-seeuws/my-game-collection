using FluentValidation;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Application.Validation;

/// <summary>
/// Validates the input of a command before its handler runs. Every command has exactly one; the command pipeline
/// rejects the command with a <see cref="CommandValidationException"/> when it finds problems.
/// </summary>
/// <remarks>Checks only what the command itself carries: rules that need stored state belong in the domain.</remarks>
public abstract class CommandInputValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICommand
{
    protected CommandInputValidatorBase()
    {
        // One message per property is enough: stop at the first rule it breaks.
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}
