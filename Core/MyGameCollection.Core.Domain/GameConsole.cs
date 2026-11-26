namespace MyGameCollection.Core.Domain;

public record GameConsole
{
    public required Guid Key { get; init; }
    public required ConsoleDetails Details { get; init; }
}
