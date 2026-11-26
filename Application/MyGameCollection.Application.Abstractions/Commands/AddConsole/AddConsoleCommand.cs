using MyGameCollection.Application.Abstractions.Contracts;

namespace MyGameCollection.Application.Abstractions.Commands.AddConsole;

public record AddConsoleCommand : ICommand
{
    public required ConsoleData Data { get; init; }
}
