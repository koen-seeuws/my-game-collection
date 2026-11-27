using FluentValidation;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Application.Business.Decorators;

/// <summary>
/// Rejects a command whose input is invalid before the decorated handler (and any transaction) runs. Every command
/// needs a validator: resolving a handler for a command without one fails.
/// </summary>
internal sealed class ValidatingCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly IValidator<TCommand> _validator;

    public ValidatingCommandHandler(ICommandHandler<TCommand> inner, IValidator<TCommand> validator)
    {
        _inner = inner;
        _validator = validator;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(command, cancellationToken);

        await _inner.HandleAsync(command, cancellationToken);
    }
}

/// <inheritdoc cref="ValidatingCommandHandler{TCommand}"/>
internal sealed class ValidatingCommandHandler<TCommand, TCommandReply> : ICommandHandler<TCommand, TCommandReply>
    where TCommand : ICommandInterfaces<TCommandReply>
{
    private readonly ICommandHandler<TCommand, TCommandReply> _inner;
    private readonly IValidator<TCommand> _validator;

    public ValidatingCommandHandler(ICommandHandler<TCommand, TCommandReply> inner, IValidator<TCommand> validator)
    {
        _inner = inner;
        _validator = validator;
    }

    public async Task<TCommandReply> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(command, cancellationToken);

        return await _inner.HandleAsync(command, cancellationToken);
    }
}

file static class ValidatorExtensions
{
    public static async Task EnsureValidAsync<TCommand>(this IValidator<TCommand> validator, TCommand command,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(command, cancellationToken);

        if (!result.IsValid)
            throw new CommandValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
    }
}
