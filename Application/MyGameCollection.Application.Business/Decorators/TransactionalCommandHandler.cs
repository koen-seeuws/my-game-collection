using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Persistence;

namespace MyGameCollection.Application.Business.Decorators;

/// <summary>Runs the decorated command handler inside a single transaction.</summary>
internal sealed class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionalCommandHandler(ICommandHandler<TCommand> inner, IUnitOfWork unitOfWork)
    {
        _inner = inner;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _inner.HandleAsync(command, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}

/// <summary>Runs the decorated command handler inside a single transaction.</summary>
internal sealed class TransactionalCommandHandler<TCommand, TCommandReply> : ICommandHandler<TCommand, TCommandReply>
    where TCommand : ICommandInterfaces<TCommandReply>
{
    private readonly ICommandHandler<TCommand, TCommandReply> _inner;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionalCommandHandler(ICommandHandler<TCommand, TCommandReply> inner, IUnitOfWork unitOfWork)
    {
        _inner = inner;
        _unitOfWork = unitOfWork;
    }

    public async Task<TCommandReply> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var reply = await _inner.HandleAsync(command, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return reply;
    }
}
