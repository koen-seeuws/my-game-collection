namespace MyGameCollection.Application.Abstractions.Features.Items;

public sealed record SearchItemsQuery(ItemFilter Filter) : IRequest<PagedResult<ItemListDto>>;
