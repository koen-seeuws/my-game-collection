namespace MyGameCollection.Application.Abstractions.Queries.GetMyGames;

public record GetMyGamesQuery : IQuery<IEnumerable<GameDto>>
{
}
