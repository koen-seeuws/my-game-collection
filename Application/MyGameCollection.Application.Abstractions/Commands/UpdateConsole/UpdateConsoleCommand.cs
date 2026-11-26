using MyGameCollection.Application.Abstractions.Contracts;

namespace MyGameCollection.Application.Abstractions.Commands.UpdateConsole;

public record UpdateConsoleCommand : ICommand
{
    public required Guid Key { get; init; }
    public required ConsoleData Data { get; init; }
}
