namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record UpdateItemCommand(Guid Id, ItemEditModel Item) : IRequest;
