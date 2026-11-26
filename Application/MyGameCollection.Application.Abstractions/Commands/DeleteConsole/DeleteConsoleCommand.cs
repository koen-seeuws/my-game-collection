namespace MyGameCollection.Application.Abstractions.Commands.DeleteConsole;

public record DeleteConsoleCommand : ICommand
{
    public required Guid Key { get; init; }
}
