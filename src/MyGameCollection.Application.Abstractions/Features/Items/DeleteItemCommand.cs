namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record DeleteItemCommand(Guid Id) : IRequest;
