using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.RedeemInvite;

namespace MyGameCollection.Application.Validation.Validators.RedeemInvite;

public sealed class RedeemInviteCommandInputValidator : CommandInputValidatorBase<RedeemInviteCommand>
{
    public RedeemInviteCommandInputValidator()
    {
        RuleFor(c => c.FirstName).RequiredText(RedeemInviteCommand.MaxNameLength);
        RuleFor(c => c.LastName).RequiredText(RedeemInviteCommand.MaxNameLength);
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Enter your email address.")
            .EmailAddress().WithMessage("Enter a valid email address.");
        RuleFor(c => c.Password).NotEmpty().WithMessage("Choose a password.");
        RuleFor(c => c.Token).NotEmpty().WithMessage("This invite link is invalid.");
    }
}
