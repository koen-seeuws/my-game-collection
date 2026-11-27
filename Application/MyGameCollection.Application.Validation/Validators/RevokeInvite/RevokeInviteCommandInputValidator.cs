using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.RevokeInvite;

namespace MyGameCollection.Application.Validation.Validators.RevokeInvite;

public sealed class RevokeInviteCommandInputValidator : CommandInputValidatorBase<RevokeInviteCommand>
{
    public RevokeInviteCommandInputValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Choose an invite.");
    }
}
