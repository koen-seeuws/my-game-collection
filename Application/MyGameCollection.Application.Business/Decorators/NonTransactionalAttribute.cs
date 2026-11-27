namespace MyGameCollection.Application.Business.Decorators;

/// <summary>
/// Leaves a command handler out of the <see cref="TransactionalCommandHandler{TCommand}"/> decorator. For handlers
/// that don't touch the game collection's unit of work and get their atomicity elsewhere: on SQLite, an open
/// collection transaction would even block the writes of another database connection until it times out.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NonTransactionalAttribute : Attribute;
