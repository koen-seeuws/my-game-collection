namespace MyGameCollection.Application.Abstractions.Features.Barcodes;

/// <summary>
/// Resolves a scanned barcode into prefill data for the item form:
/// 1. your own collection (an item with the same barcode is the best source),
/// 2. free barcode databases (in registration order),
/// 3. an optional game metadata database to fill in developer / genre / year / box art.
/// </summary>
public sealed record LookupBarcodeQuery(string Barcode) : IRequest<BarcodeLookupResult>;
