namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record GetItemDetailsQuery(Guid Id) : IRequest<ItemDetailsDto>;
