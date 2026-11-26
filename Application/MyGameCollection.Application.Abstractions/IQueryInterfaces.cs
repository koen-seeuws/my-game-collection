namespace MyGameCollection.Application.Abstractions;

public interface IQuery<TQueryReply>;

public interface IQueryHandler<TQuery, TQueryReply> where TQuery : IQuery<TQueryReply>
{
    Task<TQueryReply> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
