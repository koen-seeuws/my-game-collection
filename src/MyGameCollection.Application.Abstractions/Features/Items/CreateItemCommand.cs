namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record CreateItemCommand(ItemEditModel Item) : IRequest<Guid>;
