namespace MyGameCollection.Application.Abstractions.Commands.DeleteGame;

public record DeleteGameCommand : ICommand
{
    public required Guid Key { get; init; }
}
