namespace MyGameCollection.Application.Abstractions;

/// <summary>
/// Executes commands and queries by routing them to their registered handler.
/// </summary>
/// <remarks>
/// The handler is picked from the runtime type of the command/query, so callers never have to spell out
/// the handler's generic arguments: <c>await dispatcher.QueryAsync(new GetMyGamesQuery())</c>.
/// </remarks>
public interface IUseCaseDispatcher
{
    /// <summary>Executes a command that has no reply.</summary>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>Executes a command and returns its reply.</summary>
    Task<TReply> SendAsync<TReply>(ICommandInterfaces<TReply> command, CancellationToken cancellationToken = default);

    /// <summary>Executes a query and returns its reply.</summary>
    Task<TReply> QueryAsync<TReply>(IQuery<TReply> query, CancellationToken cancellationToken = default);
}
