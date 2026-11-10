namespace MyGameCollection.Application.Abstractions.Features.Items;

/// <summary>Items that already carry this barcode (so you notice duplicates before adding).</summary>
public sealed record FindItemsByBarcodeQuery(string Barcode) : IRequest<IReadOnlyList<ItemListDto>>;
