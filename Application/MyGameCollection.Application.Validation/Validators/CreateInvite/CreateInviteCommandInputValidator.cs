using FluentValidation;
using MyGameCollection.Application.Abstractions.Commands.CreateInvite;

namespace MyGameCollection.Application.Validation.Validators.CreateInvite;

public sealed class CreateInviteCommandInputValidator : CommandInputValidatorBase<CreateInviteCommand>
{
    public CreateInviteCommandInputValidator()
    {
        var validityMessage =
            $"An invite must be valid for some time, up to {CreateInviteCommand.MaxValidity.TotalDays} days.";

        RuleFor(c => c.ValidFor)
            .GreaterThan(TimeSpan.Zero).WithMessage(validityMessage)
            .LessThanOrEqualTo(CreateInviteCommand.MaxValidity).WithMessage(validityMessage);
        RuleFor(c => c.Note)
            .OptionalText(CreateInviteCommand.MaxNoteLength)
            .WithMessage($"Keep the note under {CreateInviteCommand.MaxNoteLength} characters.");
    }
}
