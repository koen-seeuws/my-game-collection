namespace MyGameCollection.Application.Abstractions.Commands.DeleteAccessory;

public record DeleteAccessoryCommand : ICommand
{
    public required Guid Key { get; init; }
}
