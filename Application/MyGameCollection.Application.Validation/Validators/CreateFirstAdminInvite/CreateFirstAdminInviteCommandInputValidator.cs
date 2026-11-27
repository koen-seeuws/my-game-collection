using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.CreateFirstAdminInvite;

namespace MyGameCollection.Application.Validation.Validators.CreateFirstAdminInvite;

public sealed class CreateFirstAdminInviteCommandInputValidator : CommandInputValidatorBase<CreateFirstAdminInviteCommand>
{
    public CreateFirstAdminInviteCommandInputValidator()
    {
        RuleFor(c => c.ValidFor).GreaterThan(TimeSpan.Zero).WithMessage("An invite must be valid for some time.");
    }
}
