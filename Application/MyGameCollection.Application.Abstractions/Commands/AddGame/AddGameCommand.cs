using MyGameCollection.Application.Abstractions.Contracts;

namespace MyGameCollection.Application.Abstractions.Commands.AddGame;

public record AddGameCommand : ICommand
{
    public required GameData Data { get; init; }
}
