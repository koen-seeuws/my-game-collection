namespace MyGameCollection.Application.Abstractions.Features.Barcodes;

/// <summary>Tells the UI which optional lookups are configured (e.g. a RAWG API key).</summary>
public sealed record GetLookupCapabilitiesQuery : IRequest<LookupCapabilities>;
