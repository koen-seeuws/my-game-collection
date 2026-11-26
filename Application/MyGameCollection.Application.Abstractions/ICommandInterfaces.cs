namespace MyGameCollection.Application.Abstractions;

public interface ICommand;

public interface ICommandInterfaces<TCommandReply> : ICommand;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand, TCommandReply> where TCommand : ICommandInterfaces<TCommandReply>
{
    Task<TCommandReply> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
