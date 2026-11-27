namespace MyGameCollection.Application.Abstractions.Commands.DeleteAddOn;

public record DeleteAddOnCommand : ICommand
{
    public required Guid Key { get; init; }
}
