using MyGameCollection.Application.Abstractions.Contracts;

namespace MyGameCollection.Application.Abstractions.Commands.UpdateGame;

public record UpdateGameCommand : ICommand
{
    public required Guid Key { get; init; }
    public required GameData Data { get; init; }
}
