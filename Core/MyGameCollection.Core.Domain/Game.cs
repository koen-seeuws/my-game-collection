namespace MyGameCollection.Core.Domain;

public record Game
{
    public required Guid Key { get; init; }
    public required GameDetails Details { get; init; }
}
