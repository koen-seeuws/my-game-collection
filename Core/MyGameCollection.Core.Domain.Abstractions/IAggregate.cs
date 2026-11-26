namespace MyGameCollection.Core.Domain.Abstractions;

public interface IAggregate;

public interface IAggregate<TAggregateKey> : IAggregate
{
    TAggregateKey AggregateKey { get; init; }
}
