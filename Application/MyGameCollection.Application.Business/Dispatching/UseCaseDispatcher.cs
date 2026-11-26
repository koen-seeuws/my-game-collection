using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Application.Business.Dispatching;

/// <summary>
/// Runs every command/query in its own DI scope, giving each invocation a fresh unit of work.
/// </summary>
/// <remarks>
/// A Blazor Server circuit's scope lives as long as the browser tab, so services resolved from it (like the
/// <c>DbContext</c>) would otherwise be long-lived and shared by concurrent UI events. In the Web API the extra
/// scope is harmless. Each host's <see cref="IUserContext"/> finds the current user on its own, so nothing
/// needs to be copied into the new scope.
/// </remarks>
internal sealed class UseCaseDispatcher : IUseCaseDispatcher
{
    // One strongly typed invoker per concrete command/query type, so reflection is only paid once.
    private static readonly ConcurrentDictionary<Type, object> CommandInvokers = new();
    private static readonly ConcurrentDictionary<Type, object> CommandWithReplyInvokers = new();
    private static readonly ConcurrentDictionary<Type, object> QueryInvokers = new();

    private readonly IServiceScopeFactory _scopeFactory;

    public UseCaseDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoker = (CommandInvoker)CommandInvokers.GetOrAdd(command.GetType(),
            static type => Activator.CreateInstance(typeof(CommandInvoker<>).MakeGenericType(type))!);

        await using var scope = _scopeFactory.CreateAsyncScope();
        await invoker.HandleAsync(command, scope.ServiceProvider, cancellationToken);
    }

    public async Task<TReply> SendAsync<TReply>(ICommandInterfaces<TReply> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoker = (CommandWithReplyInvoker<TReply>)CommandWithReplyInvokers.GetOrAdd(command.GetType(),
            static type => Activator.CreateInstance(
                typeof(CommandWithReplyInvoker<,>).MakeGenericType(type, typeof(TReply)))!);

        await using var scope = _scopeFactory.CreateAsyncScope();
        return await invoker.HandleAsync(command, scope.ServiceProvider, cancellationToken);
    }

    public async Task<TReply> QueryAsync<TReply>(IQuery<TReply> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoker = (QueryInvoker<TReply>)QueryInvokers.GetOrAdd(query.GetType(),
            static type => Activator.CreateInstance(
                typeof(QueryInvoker<,>).MakeGenericType(type, typeof(TReply)))!);

        await using var scope = _scopeFactory.CreateAsyncScope();
        return await invoker.HandleAsync(query, scope.ServiceProvider, cancellationToken);
    }

    private abstract class CommandInvoker
    {
        public abstract Task HandleAsync(ICommand command, IServiceProvider services, CancellationToken ct);
    }

    private sealed class CommandInvoker<TCommand> : CommandInvoker where TCommand : ICommand
    {
        public override Task HandleAsync(ICommand command, IServiceProvider services, CancellationToken ct) =>
            services.GetRequiredService<ICommandHandler<TCommand>>().HandleAsync((TCommand)command, ct);
    }

    private abstract class CommandWithReplyInvoker<TReply>
    {
        public abstract Task<TReply> HandleAsync(ICommandInterfaces<TReply> command, IServiceProvider services, CancellationToken ct);
    }

    private sealed class CommandWithReplyInvoker<TCommand, TReply> : CommandWithReplyInvoker<TReply>
        where TCommand : ICommandInterfaces<TReply>
    {
        public override Task<TReply> HandleAsync(ICommandInterfaces<TReply> command, IServiceProvider services, CancellationToken ct) =>
            services.GetRequiredService<ICommandHandler<TCommand, TReply>>().HandleAsync((TCommand)command, ct);
    }

    private abstract class QueryInvoker<TReply>
    {
        public abstract Task<TReply> HandleAsync(IQuery<TReply> query, IServiceProvider services, CancellationToken ct);
    }

    private sealed class QueryInvoker<TQuery, TReply> : QueryInvoker<TReply>
        where TQuery : IQuery<TReply>
    {
        public override Task<TReply> HandleAsync(IQuery<TReply> query, IServiceProvider services, CancellationToken ct) =>
            services.GetRequiredService<IQueryHandler<TQuery, TReply>>().HandleAsync((TQuery)query, ct);
    }
}
