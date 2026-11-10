namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record GetItemForEditQuery(Guid Id) : IRequest<ItemEditModel>;
